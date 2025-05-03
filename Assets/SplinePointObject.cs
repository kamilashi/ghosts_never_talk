using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif


namespace GNT
{
    public enum SplinePointObjectFaction
    {
        Player,
        NPC,
        All,
    }
    
    public enum SplinePointObjectType
    {
        CheckPoint,
        InteractableTeleporter,
        InteractableTrigger,
       // AutoTrigger
    }

    public abstract class SplinePointObject : MonoBehaviour
    {
        public SplinePointObjectFaction Faction = SplinePointObjectFaction.Player;
        public GroundLayer ContainingGroundLayer;
        public float DetectionRadius = -1;

        [SerializeField] protected SplinePointObjectType splinePointObjectType;

        protected int pointIndex;
        protected bool isLocked;
        protected bool isHidden;
        // here can be the parent spline

        protected void BaseAwakeSplinePointObject()
        {
            if (ContainingGroundLayer == null)
            {
                ContainingGroundLayer = this.transform.GetComponentInParent<GroundLayer>();
            }

            isLocked = false;
            isHidden = false;

            Debug.Assert(DetectionRadius > 0.0f, "DetectionRadius is not set!");
        }

        public void SetSplinePoint(int pointIndex)
        {
            this.pointIndex = pointIndex;
            SpriteRenderer spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) 
            {
                spriteRenderer.sortingOrder = ContainingGroundLayer.SpriteLayerOrder;
            }
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public int GetSplinePointIndex()
        {
            return pointIndex;
        }

        public bool IsInDetectionRange(float currentDistance)
        {
            return !isHidden && currentDistance <= DetectionRadius;
        }

        public bool CanExecuteSplineObject()
        {
            return !isLocked;
        }

        // either auto trigger/apply something or prepare for interaction (like become available)
        public virtual void ExecuteSplineObject(PlayerController playerControllerRef = null, CharacterMovement groundMovementRef = null)
        { 

        }

        public bool IsOfType(SplinePointObjectType type)
        {
            return splinePointObjectType == type;
        }
    }
}
