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

        /*     public GroundLayer GetCurrentGroundLayer()
                {
                    return ActiveGroundLayer;
                }
                public GameObject GetCurrentScreenBottomHook()
                {
                    return ActiveGroundLayer.GetScreenBottomHook();
                }


                public void SetCurrentGroundLayer(GroundLayer groundLayer)
                {
                    currentGroundLayer = groundLayer;
                }*/


        private void SwitchToLayer(int targetLayerIdx)
        {
            currentLayerIdx = targetLayerIdx;
            ActiveGroundLayer = groundLayers[targetLayerIdx];
            Debug.Log("targetLayerIdx " + targetLayerIdx);
            Camera mainCamera = GlobalData.Instance.GetActiveCamera();
            float yShift = Library.Helpers.GetDeltaYToScreenBottom(ActiveGroundLayer.ScreenBottomHook.transform.position, mainCamera.transform.position, mainCamera.fieldOfView);

            HashSet<GameObject> uniqueSceneHierarchyLinks = new HashSet<GameObject>();

            for (int backToFrontIdx = groundLayers.Count-1; backToFrontIdx >= 0; backToFrontIdx --)
            {
                GameObject sceneLayerHierarchy = groundLayers[backToFrontIdx].SceneLayerHierarchy;
                uniqueSceneHierarchyLinks.Add(sceneLayerHierarchy);
            }

            foreach (GameObject sceneLayerHierarchy in uniqueSceneHierarchyLinks)
            {
                sceneLayerHierarchy.transform.Translate(0.0f, yShift, 0.0f);
                Debug.Log("shifted " + sceneLayerHierarchy.name + " in " + yShift);
            }
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
            currentLayerIdx = sceneStartData.startLayerIdx;
            ActiveGroundLayer = groundLayers[currentLayerIdx];
            //SwitchToLayer(currentLayerIdx);
        }
    }
}