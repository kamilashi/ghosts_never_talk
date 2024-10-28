using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    //[ExecuteInEditMode]
    public class LocalUVConverter : MonoBehaviour
    {
        // Start is called before the first frame update
        void OnEnable()
        {
            SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
            
            renderer.material.SetFloat("_LocalTopPixel", renderer.sprite.rect.position.y + renderer.sprite.rect.height);
            renderer.material.SetFloat("_LocalBottomPixel", renderer.sprite.rect.position.y);
            renderer.material.SetFloat("_TextureHeightInPixels", renderer.sprite.texture.height);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}