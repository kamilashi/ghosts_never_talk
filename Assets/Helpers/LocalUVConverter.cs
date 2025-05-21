using System;
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

        [Header("Debug View:")]
        public SpriteData currentSpruteData;

        [Serializable]
        public struct SpriteData
        {
            public float localTopPixel;
            public float localBottomPixel;
            public float spriteHeightInPixels;

            public float localLeftPixel;
            public float localRightPixel;
            public float spriteWidthInPixels;

            public int textureHeightInPixels;
            public int textureWidthtInPixels;

            public void clear()
            {
                localTopPixel = 0.0f;
                localBottomPixel = 0.0f;
                spriteHeightInPixels = 0.0f;
                localLeftPixel = 0.0f;
                localRightPixel = 0.0f;
                spriteWidthInPixels = 0.0f;
                textureHeightInPixels = 0;
                textureWidthtInPixels = 0;
            }
        }

        //public RenderTexture localSpriteUVRenderTexture;
        //public ComputeShader localUVConverterComputeShader;

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
            int textHeightInpixelsInt = 0;
            GetLocalSpriteData(ref localTopPixel, ref localBottomPixel, ref textHeightInpixelsInt);
            textureHeightInPixels = (float)textHeightInpixelsInt;

            ShaderPropertySetter.SetLocalSpriteUVsEvent += SetLocalSpriteUVs;
        }
        
        void GetLocalSpriteData(ref float localTopPixelRef, ref float localBottomPixelRef, ref int textureHeightInPixelsRef)
        {
            SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            localTopPixelRef = spriteRenderer.sprite.rect.position.y + spriteRenderer.sprite.rect.height;
            localBottomPixelRef = spriteRenderer.sprite.rect.position.y;
            textureHeightInPixelsRef = spriteRenderer.sprite.texture.height;
        }
        
        void GetLocalSpriteData(ref SpriteData localSpriteDataRef)
        {
            SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            localSpriteDataRef.localTopPixel = Mathf.Round(spriteRenderer.sprite.rect.position.y + spriteRenderer.sprite.rect.height);
            localSpriteDataRef.localBottomPixel= Mathf.Round(spriteRenderer.sprite.rect.position.y);
            localSpriteDataRef.spriteHeightInPixels = Mathf.Round(spriteRenderer.sprite.rect.height);
            
            localSpriteDataRef.localLeftPixel = Mathf.Round(spriteRenderer.sprite.rect.position.x);
            localSpriteDataRef.localRightPixel= Mathf.Round(spriteRenderer.sprite.rect.position.x + spriteRenderer.sprite.rect.width);
            localSpriteDataRef.spriteWidthInPixels = Mathf.Round(spriteRenderer.sprite.rect.width);
           
            localSpriteDataRef.textureHeightInPixels = spriteRenderer.sprite.texture.height;
            localSpriteDataRef.textureWidthtInPixels = spriteRenderer.sprite.texture.width;
        }

       void SetLocalSpriteUVs()
        {
            SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

            // should be per instance
            MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
            spriteRenderer.GetPropertyBlock(materialPropertyBlock);

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

            spriteRenderer.SetPropertyBlock(materialPropertyBlock);
        }
        public void WriteInvertedSpriteDataIntoRenderTexture(ref RenderTexture targetOutput, ref ComputeShader shader)
        {
            SpriteData spriteData = new SpriteData();
            spriteData.clear();

            GetLocalSpriteData(ref spriteData);

            currentSpruteData = spriteData;

           Debug.Assert(targetOutput.height == spriteData.textureHeightInPixels);

            int writeLocalUvDataKernel = shader.FindKernel("writeVerticallyInvertedUvCoords");

            shader.SetTexture(writeLocalUvDataKernel, "localSpriteUVRenderTexture", targetOutput);

            shader.SetFloat("localTopPixel", spriteData.localTopPixel);
            shader.SetFloat("localBottomPixel", spriteData.localBottomPixel);
            shader.SetFloat("spriteHeightInPixels", spriteData.spriteHeightInPixels);

            shader.SetFloat("localLeftPixel", spriteData.localLeftPixel);
            shader.SetFloat("localRightPixel", spriteData.localRightPixel);
            shader.SetFloat("spriteWidthInPixels", spriteData.spriteWidthInPixels);

            shader.SetFloat("textureHeightInPixels", (float) spriteData.textureHeightInPixels);
            shader.SetFloat("textureWidthInPixels", (float) spriteData.textureWidthtInPixels);

            shader.Dispatch(writeLocalUvDataKernel, spriteData.textureWidthtInPixels, spriteData.textureHeightInPixels, 1);
            //shader.Dispatch(writeLocalUvDataKernel, 10, 10, 1);
        }

        public void WriteLocalUVCoordYIntoRenderTexture(ref RenderTexture targetOutput, ref ComputeShader shader)
        {
            float locTopPixel = 0.0f;
            float locBottomPixel = 0.0f;
            int textHeightInPixels = 0;

            GetLocalSpriteData(ref locTopPixel, ref locBottomPixel, ref textHeightInPixels);

            Debug.Assert(targetOutput.height == textHeightInPixels);

            int writeLocalUvCoordKernel = shader.FindKernel("writeLocalUvCoordY");

            shader.SetTexture(writeLocalUvCoordKernel, "localSpriteUVRenderTexture", targetOutput);

            shader.SetFloat("localTopPixel", locTopPixel);
            shader.SetFloat("localBottomPixel", locBottomPixel);
            shader.SetFloat("textureHeightInPixels", (float)textHeightInPixels);

            shader.Dispatch(writeLocalUvCoordKernel, 1, textHeightInPixels, 1);
        }
        public void WriteLocalUVDataIntoRenderTexture(ref RenderTexture targetOutput, ref ComputeShader shader)
        {
            float locTopPixel = 0.0f;
            float locBottomPixel = 0.0f;
            int textHeightInPixels = 0;

            GetLocalSpriteData(ref locTopPixel, ref locBottomPixel, ref textHeightInPixels);

            Debug.Assert(targetOutput.height == textHeightInPixels);

            int writeLocalUvDataKernel = shader.FindKernel("writeLocalUvCoordWithBoundsY");

            shader.SetTexture(writeLocalUvDataKernel, "localSpriteUVRenderTexture", targetOutput);

            shader.SetFloat("localTopPixel", locTopPixel);
            shader.SetFloat("localBottomPixel", locBottomPixel);
            shader.SetFloat("textureHeightInPixels", (float)textHeightInPixels);

            shader.SetFloat("topUvBoudY", (float)(locTopPixel / ((float)textHeightInPixels)));
            shader.SetFloat("bottomUvBoudY", (float)(locBottomPixel / ((float)textHeightInPixels)));

            shader.Dispatch(writeLocalUvDataKernel, 1, textHeightInPixels, 1);
        }
    }
}