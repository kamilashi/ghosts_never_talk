using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    [Serializable]
    public struct SceneStartData
    {
        public int startLayerIdx;
        public float positionOnLayer;
    }

    public enum LayerSwitchDirection
    {
        In,
        Out
    }

    public class SceneInterface : MonoBehaviour
    {

        [SerializeField]
        private SceneStartData sceneStartData; // set in the inspector only

        [SerializeField]
        private List<GroundLayer> groundLayers;

        void Awake()
        {
        }

        void Update()
        {

        }

        public SceneStartData GetSceneStartData()
        {
            return sceneStartData;
        }

        public void OnLoadInitialize()
        {
            // delegate to load funcitonality and move to globalData
            for (int index = 0; index < groundLayers.Count; index++)
            {
                groundLayers[index].GroundLayerIndex = index;
            }
        }
        
        public GroundLayer GetGroundLayer(int targetIndex)
        {
            return groundLayers[targetIndex];
        }

        public GroundLayer GetFartherOrThisGroundLayer(int currentIndex)
        {
            if (currentIndex < groundLayers.Count - 1)
            {
                return groundLayers[currentIndex + 1];
            }

            return groundLayers[groundLayers.Count - 1];
        }

        public GroundLayer GetCloserOrThisGroundLayer(int currentIndex)
        {
            if (currentIndex > 0)
            {
                return groundLayers[currentIndex - 1];
            }

            return groundLayers[0];
        }
    }
}