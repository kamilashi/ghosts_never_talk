using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    // this should be turned into the InteractibleTeleport script or be removed
    public class GroundLayerPositionMapper : MonoBehaviour
    {
        /*public void TeteportToGroundHookPosition(ref Vector3 deltaPositionTranslate, LayerMask teleporteeGroundCollisionMask, Collider2D teleporteeCollider)
        {
            GameObject positionHook = GlobalData.Instance.ActiveSceneDynamicRef.ActiveGroundLayer.ScreenBottomHook;
            deltaPositionTranslate = GlobalData.Instance.ActiveSceneDynamicRef.ActiveGroundLayer.EdgeCollider.transform.position;
            deltaPositionTranslate.x = positionHook.transform.position.x;
            float testHeight = 20.0f;
            deltaPositionTranslate.y += testHeight;

            deltaPositionTranslate.y -= GroundMovement.GetDistanceToGroundCollider(deltaPositionTranslate, testHeight, teleporteeCollider, teleporteeGroundCollisionMask);

            deltaPositionTranslate -= this.transform.position;
        }*/
    }
}