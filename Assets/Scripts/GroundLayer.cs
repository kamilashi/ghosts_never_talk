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
        [Header("Manual Setup")]
        public GameObject LayerAssetsContainer;
        public GameObject ScreenBottomHook;
        [Range(0.0f , 1.0f)] public float ShiftScale = 1.0f;
        public float BoundingHeight = -1.0f;
        public SpriteRenderer BoundingHeightReferenceSprite = null;

        [Header("Auto Setup")]
        public SceneInterface ContainerScene;
        public int SpriteLayerOrder;
        public int GroundLayerIndex = -1;

        public Pathfinding.CatmullRomSpline MovementSpline;

        private bool isShiftedDown = false;
        private float shiftDownDistance = 0.0f;

        void Awake()
        {
            if (ContainerScene == null)
            {
                ContainerScene = this.transform.GetComponentInParent<SceneInterface>();
            }

            if (MovementSpline == null)
            {
                MovementSpline = this.GetComponentInChildren<Pathfinding.CatmullRomSpline>();
            }

            Debug.Assert(! (BoundingHeight == -1 && BoundingHeightReferenceSprite == null), "Please, specify either the bounding height reference sprite, or the height in meters, this is needed for shifting down foreground layers when switching in!");

            if (BoundingHeightReferenceSprite != null)
            {
                BoundingHeight = BoundingHeightReferenceSprite.bounds.size.y;
            }
        }

        void Update()
        {
        }

        public bool IsShiftedDown()
        {
            return isShiftedDown;
        }

        public float GetShiftDownDistance()
        { 
            return shiftDownDistance; 
        }
        public float GetBoundingHeight()
        { 
            return BoundingHeight; 
        }

        public void SetIsShiftedDown(bool isShiftedDown)
        {
            this.isShiftedDown = isShiftedDown;
        }

        public void SetShiftDownDistance(float distance)
        { 
             shiftDownDistance = distance; 
        }
    }
}