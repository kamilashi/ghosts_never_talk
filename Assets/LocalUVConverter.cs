using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphics
{
    [ExecuteInEditMode]
    public class LocalUVConverter : MonoBehaviour
    {
        public float localTopPixel;
        public float localBottomPixel;
        public float textureHeightInPixels;

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

            //Finally you set the property block of the renderer
            spriteRenderer.SetPropertyBlock(materialPropertyBlock);

            /*spriteRenderer.sharedMaterial.SetFloat("_LocalTopPixel", localTopPixel);
            spriteRenderer.sharedMaterial.SetFloat("_LocalBottomPixel", localBottomPixel);
            spriteRenderer.sharedMaterial.SetFloat("_TextureHeightInPixels", textureHeightInPixels);*/
        }
    }
}