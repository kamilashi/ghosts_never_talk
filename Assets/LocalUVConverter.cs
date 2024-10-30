using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphics
{
    public class LocalUVConverter : MonoBehaviour
    {
        SpriteRenderer renderer;

        void Awake()
        {
            renderer = gameObject.GetComponent<SpriteRenderer>();

            ShaderPropertySetter.InitializeAllShaderParametersEvent += SetLocalSpriteUVs;
        }


        void Update()
        {

        }

       void SetLocalSpriteUVs()
        {
            // should be per instance
            renderer.sharedMaterial.SetFloat("_LocalTopPixel", renderer.sprite.rect.position.y + renderer.sprite.rect.height);
            renderer.sharedMaterial.SetFloat("_LocalBottomPixel", renderer.sprite.rect.position.y);
            renderer.sharedMaterial.SetFloat("_TextureHeightInPixels", renderer.sprite.texture.height);
        }
    }
}