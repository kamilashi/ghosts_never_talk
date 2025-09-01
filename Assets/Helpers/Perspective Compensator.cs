using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphics
{
    public class PerspectiveCompensator : MonoBehaviour
    {
        public Camera referenceCamera;
        public GameObject unitScaleReference;

        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        [ContextMenu("ScaleToReference")]
        public void ScaleToReference()
        {
            float distanceFromCamera = System.Math.Abs(gameObject.transform.position.z - referenceCamera.gameObject.transform.position.z);
            float referenceDistanceFromCamera = System.Math.Abs(unitScaleReference.transform.position.z - referenceCamera.gameObject.transform.position.z);
            float scaleFactor = distanceFromCamera / referenceDistanceFromCamera;
            gameObject.transform.localScale = unitScaleReference.transform.localScale * scaleFactor;
        }

        [ContextMenu("SnapToCameraBottom")]
        public void SnapToCameraBottom()
        {
            Debug.Log("The objects pivot must be at the bottom of the sprite for the script to work correctly");

            transform.Translate(0.0f, GetYTranslationToCameraBottom(this.transform, referenceCamera), 0.0f);
        }

        public static float GetYTranslationToCameraBottom(Transform transform, Camera camera)
        {
            float distanceFromCamera = System.Math.Abs(transform.position.z - camera.transform.position.z);
            float deltaYto0 = camera.transform.position.y - transform.position.y;
            float deltaY = distanceFromCamera * (float)System.Math.Tan(camera.fieldOfView * 0.5 * (System.Math.PI / 180.0));

            return -deltaY + deltaYto0;
        }
    }
}