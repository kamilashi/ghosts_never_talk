using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    //[ExecuteInEditMode]
    public class LayerProjectionInverse : MonoBehaviour
    {
        public Camera mainCamera;
        Vector3 startPosition;
        Vector3 startScale;

        private void Awake()
        {
            mainCamera = mainCamera == null ? Camera.main : mainCamera;

            startPosition = transform.position;
            startScale = transform.localScale;
        }

        void Start()
        {

        }

        void Update()
        {
        }
    }

}