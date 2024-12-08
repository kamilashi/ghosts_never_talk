 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    public struct Speed
    {
        float min;
        float max;
        float lerpProgress;
        float current;

        public float EaseOut(float lambda, float dt)
        {
            current = Library.SmoothingFuncitons.Damp(max, min, lambda, dt);
            lerpProgress += dt;
            return current;
        }

        public float EaseIn(float dt)
        {
            current = Mathf.Lerp(max, min, lerpProgress);
            lerpProgress += dt;
            return current;
        }

        public void Clear()
        {
            lerpProgress = 0.0f;
            current = 0.0f;
        }

        public Speed(float min, float max)
        {
            this.min = min;
            this.max = max;
            lerpProgress = 0.0f;
            current = 0.0f;
        }
    }

    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraMovement : MonoBehaviour
    {
        [Header("Player following")]
        public Vector2 offsetFromPlayer;
        public float cameraFollowPlayerSpeed;
        [Range(-10, 0)]
        public float defaultPlayerOffsetZ = -7.0f;
        [Range(0, 10)]
        public float directionChangeReactionSpeed = 5.0f;
        [Range(0, 5)]
        public float maxLookaheadDistanceX = 2.0f;
        public float lookaheadBuildupSpeed = 10.0f;
        [Range(0, 1)]
        public float lookaheadDirectionDampLambda = 0.75f;

        [Header("Bottom screen position")]
        public bool constantGroundLevel = false;
       // private float groundLevel = 0.0f;
        private float fitScreenBottomHookY = 0.0f;

        [Header("Height change z-dollying")]
        public bool dollyOnHeightChange = true;
        public float dollySpeed = 1.0f;
        public float maxDollyAmount = 3.0f;
        private float playerToGroundHeightDifferenceThreshold = 2.6f; // start dollying when the distance from the players y position to the ground level (ground sprites pivot point pos y) if higher than this value

        // private
        [SerializeField]
        private PlayerController playerController;
       // [SerializeField]
       // private  GroundLayer activeGroundLayerRef;
        [SerializeField]
        private Camera mainCamera;
        
        [SerializeField]
        private float lookaheadDirectionSmooth = 0.0f;

        void Start()
        {
            playerController = GlobalData.Instance.GetPlayerController(); // cash out the reference to the player controllers

            mainCamera = this.GetComponent<Camera>();
        }

        void Update()
        {
            // from - 1 to 1
            lookaheadDirectionSmooth = Mathf.Clamp(Library.SmoothingFuncitons.Damp(lookaheadDirectionSmooth, playerController.GetMoveKeyHoldScale() * (float)playerController.GetLastDirectionInput(), lookaheadDirectionDampLambda, directionChangeReactionSpeed * Time.deltaTime), -1.0f, 1.0f);

            // from 0 (stopped) to -1 or 1
            float lookaheadDistanceScaled = lookaheadDirectionSmooth * maxLookaheadDistanceX /** lookaheadBuildupSpeed*/;

            // read it from ground movement or some other interface?
            Vector3 thisFramePlayerPosition = playerController.gameObject.transform.position; 

            Vector3 predictedPosition = thisFramePlayerPosition;
            predictedPosition.z += defaultPlayerOffsetZ;
            predictedPosition.x += lookaheadDistanceScaled;

            float playeFollowPositionY = thisFramePlayerPosition.y;

            float heightDifferenceThreshold = playerToGroundHeightDifferenceThreshold;
            float heightReference = 0.0f;

            GroundLayer activeGroundLayerRef = GlobalData.Instance.ActiveScene.ActiveGroundLayer;
            if (constantGroundLevel) // cameras y position relative to ground
            {
                float referenceExtentY = (activeGroundLayerRef.ScreenBottomHook.transform.position.z - transform.position.z) * (float)System.Math.Tan(mainCamera.fieldOfView * 0.5 * (System.Math.PI / 180.0));
                fitScreenBottomHookY = activeGroundLayerRef.ScreenBottomHook.transform.position.y + referenceExtentY;
                predictedPosition.y = fitScreenBottomHookY;

                heightReference = fitScreenBottomHookY;
            }
            else // make cameras y position be relative to the player
            {
                predictedPosition.y += offsetFromPlayer.y;

                //heightDifferenceThreshold = offsetFromPlayer.y;
                heightReference = activeGroundLayerRef.ScreenBottomHook.transform.position.y + playerToGroundHeightDifferenceThreshold + offsetFromPlayer.y;
            }

            if (dollyOnHeightChange)
            {
                // Hack: should be triggered by the ground movement nodes, and not depend on absolute y position, but for now
                // dolly the camera back when going up (reference ground y = 0)
                float dollyAmount = System.Math.Min(System.Math.Abs(heightReference - (playeFollowPositionY + heightDifferenceThreshold)), maxDollyAmount);
                float dollyDirection = System.Math.Sign(heightReference - (playeFollowPositionY + heightDifferenceThreshold));

                // todo: define the default camera z offset in the ground layer data!
                predictedPosition.z += dollyDirection * dollyAmount;
            }

            Vector3 currrentCameraPosition = this.gameObject.transform.position;
            Vector3 deltaPosition = predictedPosition; //Library.SmoothingFuncitons.ApproachReferenceLinear(currrentCameraPosition, predictedPosition, new Vector3(cameraFollowPlayerSpeed * Time.deltaTime, cameraFollowPlayerSpeed * Time.deltaTime, dollySpeed* Time.deltaTime));
            deltaPosition -= currrentCameraPosition;

            //deltaPosition = Vector3.Lerp(deltaPosition * cameraFollowPlayerSpeed * Time.deltaTime, deltaPosition, playerController.GetMoveKeyHoldScale());
            //deltaPosition *=  cameraFollowPlayerSpeed * Time.deltaTime;

            gameObject.transform.Translate(deltaPosition);
        }
    }
}