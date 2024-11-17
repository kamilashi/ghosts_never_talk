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
        [Range(0, 10)]
        public float directionChangeReactionSpeed = 5.0f;
        [Range(0, 5)]
        public float maxLookaheadDistanceX = 2.0f;

        [Header("Bottom screen position")]
        public bool constantGroundLevel = false;
        private float groundLevel = 0.0f;
        private GameObject currentForemostLayerReference;

        [Header("Height change z-dollying")]
        public float dollySpeed = 1.0f;
        public float maxDollyAmount = 3.0f;
        public float playerToGroundHeightDifferenceThreshold = 2.6f; // start dollying when the distance from the players y position to the ground level (ground sprites pivot point pos y) if higher than this value

        // private
        private PlayerController playerController;
        private Camera thisCameraReference;

        private float directionSmoothed = 0.0f;
        private float defaultCameraPosY;

        void Start()
        {
            playerController = GlobalData.Instance.playerController; // cash out the reference to the player controllers

            currentForemostLayerReference = GlobalData.Instance.activeScene.GetCurrentForemostLayer();

            thisCameraReference = GetComponent<Camera>();

            defaultCameraPosY = transform.position.y;

            Debug.Log("All sprites' pivots must be a the bottom!");
        }

        void Update()
        {
            // #todo: configurable camera acceleration?
            directionSmoothed = Library.SmoothingFuncitons.ApproachReferenceLinear(directionSmoothed, (float)playerController.GetLastDirectionInput(), directionChangeReactionSpeed*Time.deltaTime);
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
                float referenceExtentY = (currentForemostLayerReference.transform.position.z - transform.position.z) * (float)System.Math.Tan(thisCameraReference.fieldOfView * 0.5 * (System.Math.PI / 180.0));
                groundLevel = currentForemostLayerReference.transform.position.y + referenceExtentY;
                predictedPosition.y = groundLevel;

                heightReference = groundLevel;
            }
            else // make cameras y position be relative to the player
            {
                // should be an instant snap, so y follow to offset from player does not need scaling
                predictedPosition.y += offsetFromPlayer.y;
                
                heightDifferenceThreshold = offsetFromPlayer.y;
                heightReference = defaultCameraPosY;
            }

            Vector3 currrentCameraPosition = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y, this.gameObject.transform.position.z);
            Vector3 deltaPosition = predictedPosition;
            deltaPosition -= currrentCameraPosition;

            // Hack: should be triggered by the ground movement nodes, and not depend on absolute y position, but for now
            // dolly the camera back when going up (reference ground y = 0)
           float dollyAmount = System.Math.Min( System.Math.Abs(playeFollowPositionY + heightDifferenceThreshold - heightReference), maxDollyAmount);
            float dollyDirection = System.Math.Sign(transform.position.y - (playeFollowPositionY + heightDifferenceThreshold));

            // the y change in predicted position will already be scaled by deltatime, since it comes from the player movement - so it's safe to just follow it on the z axis
            deltaPosition.z = 0.0f + dollyDirection * dollyAmount * /*Time.deltaTime **/ dollySpeed;

            gameObject.transform.Translate(deltaPosition);
        }
    }
}