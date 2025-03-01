using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GNT
{
    public class InteractableTeleporter : Interactable
    {
        [Header("Teleporter")]
        public InteractableTeleporter TargetTeleporter;
        public bool IsReceiverOnly;

        private void Awake()
        {
            BaseAwake();
        }

        private void Start()
        {
            // #todo: Put the call to this method into the base class and override OnVIsible and OnInvisible in the child classes!
            OnBecameVisible();
        }

        void OnBecameVisible()
        {
            if (!IsReceiverOnly)
            {
                GlobalData.Instance.ActiveSceneDynamicRef.AddPlayerVisibleTeleporter(this);
            }
        }
        void OnBecameInvisible()
        {
            GlobalData.Instance.ActiveSceneDynamicRef.RemovePlayerVisibleTeleporter(this);
        }

        public override void OnBecomeAvailable()
        {
           // Debug.Log("Teleport available");
            base.OnBecomeAvailable();
        }
        public override void OnBecomeUnavailable()
        {
           // Debug.Log("Teleport unavailable");
            base.OnBecomeUnavailable();
        }

        /*public Vector3 TeleportToTargetPosition(Transform teleporteeTransform, LayerMask teleporteeGroundCollisionMask, Collider2D teleporteeCollider, SpriteRenderer teleporteeSpriteRenderer)
        {
            teleporteeSpriteRenderer.sortingOrder = TargetTeleporter.ContainingGroundLayer.SpriteLayerOrder;

            Vector3 deltaPositionTranslate = TargetTeleporter.transform.position;
            float testHeight = 10.0f;
            deltaPositionTranslate.y -= GroundMovement.GetDistanceToGroundCollider(deltaPositionTranslate, testHeight, teleporteeCollider, teleporteeGroundCollisionMask);
            deltaPositionTranslate -= teleporteeTransform.transform.position;

            return deltaPositionTranslate;
        }*/
        
        public void Teleport(ref SpriteRenderer teleporteeSpriteRenderer, ref GroundMovement teleporteeGroundMovement)
        {
            teleporteeSpriteRenderer.sortingOrder = TargetTeleporter.ContainingGroundLayer.SpriteLayerOrder;

            teleporteeGroundMovement.TeleportToSplinePoint(MAPPED_SPLINE_NODE_INDEX);
        }
    }
}