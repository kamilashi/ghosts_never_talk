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
    }

    public class SceneInterface : MonoBehaviour
    {
        public GroundLayer ActiveGroundLayer; // for now set inspector - later should be loaded + managed during the switch

        [SerializeField]
        private SceneStartData sceneStartData; // set in the inspector only

        [SerializeField]
        private List<GroundLayer> groundLayers;

        private int currentLayerIdx;

        void Start()
        {
            
        }

        void Update()
        {

        }

        public void SwitchToLayer(int targetLayerIdx)
        {
            // as soon as colliders are replaced with a waling path, this needs to change. Currently any other collider users besides the plazer will break, as all ground colliders that are unused by the player will be disabled.
            foreach( GroundLayer layer in groundLayers)
            {
                layer.EdgeCollider.enabled = false;
            }

            currentLayerIdx = targetLayerIdx;
            ActiveGroundLayer = groundLayers[targetLayerIdx];


            ActiveGroundLayer.EdgeCollider.enabled = true;

            Debug.Log("targetLayerIdx " + targetLayerIdx);
        }

        public bool SwitchIn()
        {
            if (currentLayerIdx < groundLayers.Count-1)
            {
                SwitchToLayer(currentLayerIdx + 1);
                return true;
            }

            return false;
        }

        public bool SwitchOut()
        {
            if (currentLayerIdx > 0)
            {
                SwitchToLayer(currentLayerIdx - 1);
                return true;
            }

            return false;
        }

        public void OnLoadInitialize()
        {
            // delegate to load funcitonality and move to globalData
            
            SwitchToLayer(sceneStartData.startLayerIdx);
        }
    }
}