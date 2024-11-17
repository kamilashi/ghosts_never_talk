using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    [Serializable]
    public struct SceneStartData
    {
        public GroundLayer startGroundLayerReference;
    }

    public class SceneInterface : MonoBehaviour
    {
        [SerializeField]
        private GroundLayer currentGroundLayer; // for now set inspector - later should be loaded + managed during the switch

        [SerializeField]
        private GameObject currentForemostLayer; // for now set inspector - later should be loaded + managed during he switch

        [SerializeField]
        private SceneStartData sceneStartData; // set in the inspector only

        private List<GroundLayer> groundLayers;
        private GameObject thisGameObject;

        void Awake()
        {
            groundLayers = new List<GroundLayer>();

            foreach (Transform child in gameObject.transform)
            {
                GroundLayer groundLayerInHierarchy = child.GetComponentInChildren<GroundLayer>();
                groundLayers.Add(groundLayerInHierarchy);
            }

            // delegate to load funcitonality
            currentGroundLayer = sceneStartData.startGroundLayerReference;
        }

        void Update()
        {

        }

        public GroundLayer GetCurrentGroundLayer()
        {
            return currentGroundLayer;
        }
        public GameObject GetCurrentForemostLayer()
        {
            return currentForemostLayer;
        }
    }
}