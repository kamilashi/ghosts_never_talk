using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphics
{
    [ExecuteInEditMode]
    public class LocalUVConverter : MonoBehaviour
    {
        private float localTopPixel;
        private float localBottomPixel;
        private float textureHeightInPixels;

        [Header("Sprite Local Bottom Fade")]
        public bool bottomFadeEnabled = false;
        public bool overrideGlobalBottomFade;
        [Range(0, 1)]
        public float yCutOff = 0.0f;
        [Range(0, 1)]
        public float fadeOutStart = 0.12f;
        [Range(0, 1)]
        public float fadeOutEnd = 0.0f;

        [Header("Sprite Local Distance Fade")]
        public bool distanceFadeEnabled = true;
        public bool overrideGlobalDistanceFade;
        [Range(0, 1)]
        public float intensity = 0.8f;
        public float farClipMofidier = 0.0f;
        public float nearClipModifier = 0.0f;


        //[ExecuteInEditMode]
        void Awake()
        {
            FetchLocalSpriteData();
            ShaderPropertySetter.SetLocalSpriteUVsEvent += SetLocalSpriteUVs;
        }


        [ExecuteInEditMode]
        private void Update()
        {
            SetLocalSpriteUVs();
        }

        void FetchLocalSpriteData()
        {
            SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            localTopPixel = spriteRenderer.sprite.rect.position.y + spriteRenderer.sprite.rect.height;
            localBottomPixel = spriteRenderer.sprite.rect.position.y;
            textureHeightInPixels = spriteRenderer.sprite.texture.height;

            ShaderPropertySetter.SetLocalSpriteUVsEvent += SetLocalSpriteUVs;
        }

       void SetLocalSpriteUVs()
        {
            SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            // should be per instance
            MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
            spriteRenderer.GetPropertyBlock(materialPropertyBlock);

            //Then you tweak the values in the Material Property Block
            materialPropertyBlock.SetFloat("_LocalTopPixel", localTopPixel);
            materialPropertyBlock.SetFloat("_LocalBottomPixel", localBottomPixel);
            materialPropertyBlock.SetFloat("_TextureHeightInPixels", textureHeightInPixels);

            materialPropertyBlock.SetFloat("_Sprite_BottomFade_Enabled", bottomFadeEnabled ? 1.0f : 0.0f);
            if (overrideGlobalBottomFade)
            {
                materialPropertyBlock.SetFloat("_Sprite_BottomFade_FadeOutStart", fadeOutStart);
                materialPropertyBlock.SetFloat("_Sprite_BottomFade_FadeOutEnd", fadeOutEnd);
                materialPropertyBlock.SetFloat("_Sprite_BottomFade_YOffset", yCutOff);
            }
            materialPropertyBlock.SetFloat("Sprite_DistanceFade_Enabled", distanceFadeEnabled ? 1.0f : 0.0f);
            if (overrideGlobalDistanceFade)
            {
                materialPropertyBlock.SetFloat("_Sprite_DistanceFade_Intensity", intensity);
                materialPropertyBlock.SetFloat("_Sprite_DistanceFade_FarClipModifier", farClipMofidier);
                materialPropertyBlock.SetFloat("_Sprite_DistanceFade_NearClipModifier", nearClipModifier);
            }

            //Finally you set the property block of the renderer
            spriteRenderer.SetPropertyBlock(materialPropertyBlock);
        }

/*
        [ContextMenu("FetchAndSetLocalUVData")]
        public void FetchAndSetLocalUVData()
        {
            FetchLocalSpriteData();
            SetLocalSpriteUVs();
        }*/
    }
}