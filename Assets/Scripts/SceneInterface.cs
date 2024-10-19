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
        private GroundLayer currentGroundLayer; // sett in the inspector only

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

        public GroundLayer GetGroundLayer()
        {
            return currentGroundLayer;
        }
    }
}