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
        public Vector2 playerFollowThreshold; // Camera starts following the player only if the player moves outside of this box
        public float cameraFollowPlayerSpeed = 2.0f;
        [Range(0, 10)]
        public float directionChangeReactionSpeed = 5.0f;
        [Range(0, 5)]
        public float maxLookaheadDistanceX = 2.0f;

        [Header("Bottom screen position")]
        public bool constantGroundLevel = false;
        private float groundLevel = 0.0f;

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

        private float directionSmoothed = 0.0f;
        private float defaultFromPlayerOffsetZ;

        void Start()
        {
            playerController = GlobalData.Instance.GetPlayerController(); // cash out the reference to the player controllers

            mainCamera = this.GetComponent<Camera>();

            defaultFromPlayerOffsetZ = transform.position.z - playerController.gameObject.transform.position.z;

            //playerToGroundHeightDifferenceThreshold = offsetFromPlayer.y;
        }

        void Update()
        {
            // #todo: configurable camera acceleration?
            directionSmoothed = Library.SmoothingFuncitons.ApproachReferenceLinear(directionSmoothed, (float)playerController.GetLastDirectionInput(), directionChangeReactionSpeed  * Time.deltaTime);
            float lookaheadDistanceScaled = directionSmoothed * playerController.GetMoveKeyHoldScale() * maxLookaheadDistanceX;

            // read it from ground movement or some other interface?
            Vector2 thisFramePlayerPosition = new Vector2(playerController.gameObject.transform.position.x, playerController.gameObject.transform.position.y); 

             Vector2 predictedPosition = thisFramePlayerPosition;
            predictedPosition.x += lookaheadDistanceScaled;
            predictedPosition.x += offsetFromPlayer.x * directionSmoothed;

           float playeFollowPositionY = thisFramePlayerPosition.y;

            float heightDifferenceThreshold = playerToGroundHeightDifferenceThreshold;
            float heightReference = 0.0f;

            if (constantGroundLevel) // cameras y position relative to ground
            {
                GroundLayer activeGroundLayerRef = GlobalData.Instance.ActiveScene.ActiveGroundLayer;

                float referenceExtentY = (activeGroundLayerRef.ScreenBottomHook.transform.position.z - transform.position.z) * (float)System.Math.Tan(mainCamera.fieldOfView * 0.5 * (System.Math.PI / 180.0));
                groundLevel = activeGroundLayerRef.ScreenBottomHook.transform.position.y + referenceExtentY;
                predictedPosition.y = groundLevel;

                heightReference = groundLevel;
            }
            else // make cameras y position be relative to the player - does not work
            {
                // should be an instant snap, so y follow to offset from player does not need scaling
                predictedPosition.y += offsetFromPlayer.y;
                
                heightDifferenceThreshold = offsetFromPlayer.y;
                heightReference = thisFramePlayerPosition.y;
            }

            Vector3 currrentCameraPosition = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y, this.gameObject.transform.position.z);
            Vector3 deltaPosition = predictedPosition;
            deltaPosition -= currrentCameraPosition;
            deltaPosition.z = 0.0f;
            deltaPosition *= cameraFollowPlayerSpeed * Time.deltaTime;

            if (dollyOnHeightChange)
            {
                // Hack: should be triggered by the ground movement nodes, and not depend on absolute y position, but for now
                // dolly the camera back when going up (reference ground y = 0)
                float dollyAmount = System.Math.Min(System.Math.Abs(playeFollowPositionY + heightDifferenceThreshold - heightReference), maxDollyAmount);
                float dollyDirection = System.Math.Sign(transform.position.y - (playeFollowPositionY + heightDifferenceThreshold));

                // todo: define the default camera z offset in the ground layer data!
                deltaPosition.z = 0.0f + dollyDirection * dollyAmount * Time.deltaTime * dollySpeed;
            }

            gameObject.transform.Translate(deltaPosition);
        }
    }
}