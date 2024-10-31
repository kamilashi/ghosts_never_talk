using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphics
{
    public class SpriteScaler : MonoBehaviour
    {
        public Camera referenceCamera;
        public SpriteRenderer unitScaleSpriteReference;
        float unitDistance;
        void Start()
        {
            unitDistance = System.Math.Abs(unitScaleSpriteReference.gameObject.transform.position.z - referenceCamera.gameObject.transform.position.z);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ScaleChildSprites()
        {
            SpriteRenderer[] sprites;

            sprites = this.gameObject.GetComponentsInChildren<SpriteRenderer>();

            foreach (SpriteRenderer sprite in sprites)
            {
                float distanceFromCamera = System.Math.Abs(sprite.gameObject.transform.position.z - referenceCamera.gameObject.transform.position.z);
                float scale = distanceFromCamera / unitDistance;
                sprite.transform.localScale = (new Vector3(unitScaleSpriteReference.transform.localScale.x * scale, unitScaleSpriteReference.transform.localScale.y * scale, 1.0f));
            }
        }
    }
}