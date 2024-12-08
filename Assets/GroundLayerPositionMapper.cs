using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    [RequireComponent(typeof(GroundMovement))]
    public class GroundLayerPositionMapper : MonoBehaviour
    {
        private Collider2D collider;
        private GroundMovement groundMovement;

        // Start is called before the first frame update
        void Start()
        {
            collider = this.GetComponent<Collider2D>();
            groundMovement = this.GetComponent<GroundMovement>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void TeteportToGroundHookPosition(ref float distanceSquareRef)
        {
            GameObject positionHook = GlobalData.Instance.ActiveScene.ActiveGroundLayer.ScreenBottomHook;
            Vector3 deltaPositionTranslate = GlobalData.Instance.ActiveScene.ActiveGroundLayer.EdgeCollider.transform.position;
            deltaPositionTranslate.x = positionHook.transform.position.x;
            float testHeight = 20.0f;
            deltaPositionTranslate.y += testHeight;

            deltaPositionTranslate.y -= GroundMovement.GetDistanceToGroundCollider(deltaPositionTranslate, testHeight, collider, groundMovement.GroundCollisionMask);

            deltaPositionTranslate -= this.transform.position;

            distanceSquareRef = deltaPositionTranslate.sqrMagnitude;

            groundMovement.TeleportWithAnimation(deltaPositionTranslate /*, animationKey */ );
        }
    }
}