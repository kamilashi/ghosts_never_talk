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
            float distanceFromCamera = System.Math.Abs(gameObject.transform.position.z - referenceCamera.gameObject.transform.position.z);
            float deltaY = distanceFromCamera * (float)System.Math.Tan(referenceCamera.fieldOfView * 0.5 * (System.Math.PI / 180.0));
            transform.Translate(0.0f,-deltaY, 0.0f);
        }
    }
}