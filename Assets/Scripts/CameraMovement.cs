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
        public float cameraFollowPlayerMovementDampLambda = 15.0f;
        public bool useConstantFollowPlayerDampLambda = true;
        public float cameraFollowPlayerTeleportDampLambda = 5.0f;
        public float followPlayerDampLambdaUpdateSpeed = 5.5f;
        [Range(-10, 0)]
        public float defaultPlayerOffsetZ = -7.0f;

        [Header("Horizontal lookahead")]
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

        private float dollyAmountExternal = 0.0f;

        // private
        [SerializeField]
        private float currentFollowPlayerDampLambda;
        private PlayerController playerController;
        private Camera mainCamera;
        private float lookaheadDirectionSmooth = 0.0f;
        private bool isCameraMovementEnabled = true;
        private void Awake()
        {
            currentFollowPlayerDampLambda = 0.0f;
        }

        void Start()
        {
            playerController = GlobalData.Instance.GetPlayerController(); // cash out the constant reference to the player controller

            mainCamera = this.GetComponent<Camera>(); 
        }

        void Update()
        {
            if(isCameraMovementEnabled)
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

            float predictedCameraPositionZWithoutDolly = predictedCameraPosition.z;
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

                // external dolly requests:
                predictedCameraPosition.z += dollyAmountExternal;

                Vector3 currrentCameraPosition = this.gameObject.transform.position;

            float lerpedCameraFollowPlayerDampLambda;
            if (useConstantFollowPlayerDampLambda)
            {
                lerpedCameraFollowPlayerDampLambda = cameraFollowPlayerMovementDampLambda;
            }
            else
            {
                currentFollowPlayerDampLambda = Library.SmoothingFuncitons.ApproachReferenceLinear(currentFollowPlayerDampLambda, cameraFollowPlayerMovementDampLambda, followPlayerDampLambdaUpdateSpeed * Time.deltaTime);
                lerpedCameraFollowPlayerDampLambda = currentFollowPlayerDampLambda;
            }

            Vector3 deltaPosition = Library.SmoothingFuncitons.Damp(currrentCameraPosition, predictedCameraPosition, new Vector3(lerpedCameraFollowPlayerDampLambda, lerpedCameraFollowPlayerDampLambda, dollyDampLambda), Time.deltaTime);
            deltaPosition -= currrentCameraPosition;

            gameObject.transform.Translate(deltaPosition);

            }
        }

        private void SetCurrentPlayerFollowDampLambda(float dampLambdaOverride)
        {
            currentFollowPlayerDampLambda = dampLambdaOverride;
        }
        
        public void SetPlayerFollowTeleportDampLambda()
        {
            SetCurrentPlayerFollowDampLambda (cameraFollowPlayerTeleportDampLambda);
        }

        public void SetPlayerFollowEnabled(bool isEnabled)
        {
            isCameraMovementEnabled = isEnabled;
        }
        
        public void Dolly(float dollyAmount /*In: + Out: -*/)
        {
            dollyAmountExternal = dollyAmount;
        }
        
        public void ResetDolly()
        {
            dollyAmountExternal = 0.0f;
        }
    }
}