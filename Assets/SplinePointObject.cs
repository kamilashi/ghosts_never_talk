using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;


namespace GNT
{
    public enum SplinePointObjectTriggerType
    {
        Custom,
        AutoTriggerPlayer,
        AutoTriggerNPC,
        AutoTriggerAll,
    }
    
    public enum SplinePointObjectType
    {
        CheckPoint,
        InteractableTeleporter,
        InteractableTrigger
    }

    public abstract class SplinePointObject : MonoBehaviour
    {
        public SplinePointObjectTriggerType TriggerType = SplinePointObjectTriggerType.Custom;
        public GroundLayer ContainingGroundLayer;
        public float DetectionRadius;

        [SerializeField] protected SplinePointObjectType splinePointObjectType;

        protected int splinePointIdx;
        protected bool isLocked;
        // here can be the parent spline

        protected void BaseAwakeSplinePointObject()
        {
            if (ContainingGroundLayer == null)
            {
                ContainingGroundLayer = this.transform.GetComponentInParent<GroundLayer>();
            }

            isLocked = false;
        }

        public void SetSplinePoint(int pointIndex)
        {
            splinePointIdx = pointIndex;
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public int GetSplinePoint()
        {
            return splinePointIdx;
        }

        public bool IsInDetectionRange(float currentDistance)
        {
            return currentDistance <= DetectionRadius;
        }

        public bool CanExecuteSplineObject()
        {
            return !isLocked;
        }

        public virtual void ExecuteSplineObject(PlayerController playerControllerRef = null, GroundMovement groundMovementRef = null)
        { 

        }

        public bool IsOfType(SplinePointObjectType type)
        {
            return splinePointObjectType == type;
        }
    }
}
