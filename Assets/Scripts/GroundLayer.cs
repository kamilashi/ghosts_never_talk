using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
/// <summary>
/// This class will most likely hold and maybe process the ground movement path for the entire walkable area of this layer
/// </summary>
    public class GroundLayer : SystemObject
    {
        public SceneInterface ContainerScene;
        public GameObject LayerAssetsContainer;
        public GameObject ScreenBottomHook;
        public int SpriteLayerOrder;

        public EdgeCollider2D EdgeCollider;

        void Start()
        {
        }

        void Update()
        {
        }
    }
}