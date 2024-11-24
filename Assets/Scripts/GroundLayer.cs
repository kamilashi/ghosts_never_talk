using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    public class GroundLayer : MonoBehaviour
    {
        public SceneInterface ContainerScene;
        public GameObject SceneLayerHierarchy;
        public GameObject ScreenBottomHook;
        //public GameObject WalkableSprite;

        private GlobalData globalDataInstance;

        void Start()
        {
            globalDataInstance = GlobalData.Instance;
        }

        void Update()
        {

        }

        public GameObject GetScreenBottomHook()
        {
            return ScreenBottomHook;
        }
    }

}