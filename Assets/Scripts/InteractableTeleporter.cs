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


        private void Start()
        {
            /*if (!IsReceiverOnly)
            {
                GlobalData.Instance.ActiveScene.AddPlayerVisibleTeleporter(this);
            }*/
            OnBecameVisible();
        }

        public Vector3 TeteportToTargetPosition(Transform teleporteeTransform, LayerMask teleporteeGroundCollisionMask, Collider2D teleporteeCollider, SpriteRenderer teleporteeSpriteRenderer)
        {
            teleporteeSpriteRenderer.sortingOrder = TargetTeleporter.ContainingGroundLayer.SpriteLayerOrder;

            Vector3 deltaPositionTranslate = TargetTeleporter.transform.position;
            float testHeight = 10.0f;
            //deltaPositionTranslate.y += testHeight;
            deltaPositionTranslate.y -= GroundMovement.GetDistanceToGroundCollider(deltaPositionTranslate, testHeight, teleporteeCollider, teleporteeGroundCollisionMask);
            deltaPositionTranslate -= teleporteeTransform.transform.position;

            //teleporteeTransform.Translate(deltaPositionTranslate);
            return deltaPositionTranslate;
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

    }
}