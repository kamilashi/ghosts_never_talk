 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraMovement : MonoBehaviour
    {
        [Header("Player following")]
        public Vector2 offsetFromPlayer;
        public float cameraFollowPlayerMovementDampLambda;
        public bool useMovementDampLambdaOnly = true;
        public float cameraFollowPlayerTeleportDampLambda;
        [Range(-10, 0)]
        public float defaultPlayerOffsetZ = -7.0f;
        [Range(0, 10)]
        public float directionChangeReactionSpeed = 5.0f;
        [Range(0, 5)]
        public float maxLookaheadDistanceX = 2.0f;
        public float lookaheadDirectionDampLambda = 0.75f;

        [Header("Bottom screen position")]
        public bool adjustToScreenBottomHook = false;
        private float fitScreenBottomHookY = 0.0f;

        [Header("Height change z-dollying")]
        public bool dollyOnHeightChange = true;
        public float dollyDampLambda = 0.8f;
        public float maxDollyAmount = 3.0f;
        public float playerToGroundHeightDifferenceThreshold = 2.6f; // start dollying when the distance from the players y position to the ground level (ground sprites pivot point pos y) if higher than this value

        // private
        [SerializeField]
        private float considerTeleportDistaceSquareMin = 0.5f;
        [SerializeField]
        private float considerTeleportDistaceSquareMax = 5.0f;
        [SerializeField]
        private float teleportingToMovingLerpValue;
        private PlayerController playerController;
        private Camera mainCamera;
        private float lookaheadDirectionSmooth = 0.0f;

        void Start()
        {
            playerController = GlobalData.Instance.GetPlayerController(); // cash out the constant reference to the player controller

            mainCamera = this.GetComponent<Camera>(); // should probably get the active camera from runtime; no guarantee that this reference will stay constant
        }

        void Update()
        {
            // from - 1 to 1, 0 = not moving
            lookaheadDirectionSmooth = Mathf.Clamp(Library.SmoothingFuncitons.Damp(lookaheadDirectionSmooth, playerController.GetMoveKeyHoldScale() * (float)playerController.GetLastDirectionInput(), lookaheadDirectionDampLambda, directionChangeReactionSpeed * Time.deltaTime), -1.0f, 1.0f);

            // from -maxLookaheadDistanceX to maxLookaheadDistanceX
            float lookaheadDistanceScaled = lookaheadDirectionSmooth * maxLookaheadDistanceX;

            Vector3 thisFramePlayerPosition = playerController.gameObject.transform.position; 
            Vector3 predictedCameraPosition = thisFramePlayerPosition;
            predictedCameraPosition.z += defaultPlayerOffsetZ;
            predictedCameraPosition.x += lookaheadDistanceScaled;

            float cameraHeightChangeThreshold = playerToGroundHeightDifferenceThreshold;
            float cameraHeightChangeReference = 0.0f;

            GroundLayer activeGroundLayerRef = GlobalData.Instance.ActiveScene.ActiveGroundLayer;
            if (adjustToScreenBottomHook) // cameras y position relative to ground
            {
                float referenceExtentY = (activeGroundLayerRef.ScreenBottomHook.transform.position.z - transform.position.z) * (float)System.Math.Tan(mainCamera.fieldOfView * 0.5 * (System.Math.PI / 180.0));
                fitScreenBottomHookY = activeGroundLayerRef.ScreenBottomHook.transform.position.y + referenceExtentY;
                predictedCameraPosition.y = fitScreenBottomHookY;

                cameraHeightChangeReference = fitScreenBottomHookY;
            }
            else // make cameras y position be relative to the player
            {
                predictedCameraPosition.y += offsetFromPlayer.y;

                cameraHeightChangeReference = activeGroundLayerRef.ScreenBottomHook.transform.position.y + playerToGroundHeightDifferenceThreshold + offsetFromPlayer.y;
            }

            if (dollyOnHeightChange)
            {
                // Hack: should be triggered by the ground movement nodes, and not depend on absolute y position, but for now
                // dolly the camera back when going up (reference ground y = 0)
                float playerPosDifferenceY = cameraHeightChangeReference - (thisFramePlayerPosition.y + cameraHeightChangeThreshold);
                float dollyAmount = System.Math.Min(System.Math.Abs(playerPosDifferenceY), maxDollyAmount);
                float dollyDirection = System.Math.Sign(playerPosDifferenceY);

                // todo: define the default camera z offset in the ground layer data!
                predictedCameraPosition.z += dollyDirection * dollyAmount;
            }

            Vector3 currrentCameraPosition = this.gameObject.transform.position;

            float lerpedCameraFollowPlayerDampLambda;
            if (useMovementDampLambdaOnly)
            {
                lerpedCameraFollowPlayerDampLambda = cameraFollowPlayerMovementDampLambda;
            }
            else
            {
                Vector3 deltaPositionAbsolute = predictedCameraPosition;
                deltaPositionAbsolute -= currrentCameraPosition;
                float distanceSquare = deltaPositionAbsolute.sqrMagnitude;
                float linearDecay = ((distanceSquare - considerTeleportDistaceSquareMin) / considerTeleportDistaceSquareMax - considerTeleportDistaceSquareMin);
                teleportingToMovingLerpValue = linearDecay;
               // teleportingToMovingLerpValue = Library.SmoothingFuncitons.Damp(teleportingToMovingLerpValue, Mathf.Lerp(cameraFollowPlayerMovementDampLambda, cameraFollowPlayerTeleportDampLambda, linearDecay), 0.2f, Time.deltaTime);
                lerpedCameraFollowPlayerDampLambda = Mathf.Lerp(cameraFollowPlayerMovementDampLambda, cameraFollowPlayerTeleportDampLambda, teleportingToMovingLerpValue);
            }

            Vector3 deltaPosition = Library.SmoothingFuncitons.Damp(currrentCameraPosition, predictedCameraPosition, new Vector3(lerpedCameraFollowPlayerDampLambda, lerpedCameraFollowPlayerDampLambda, dollyDampLambda), Time.deltaTime);
            deltaPosition -= currrentCameraPosition;

            gameObject.transform.Translate(deltaPosition);
        }

        // experimental:
/*
       public void SetConsiderTeleportSquareDistanceMax(float distanceSquare)
        {
            considerTeleportDistaceSquareMax = Mathf.Max(distanceSquare, considerTeleportDistaceSquareMin);
        }*/
    }
}