using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    public class CameraMovement : MonoBehaviour
    {
        [SerializeField]
        [Range(0, 1)]
        private float riseLerpValue = 0.8f;
        [SerializeField]
        [Range(0, 1)]
        private float fallLerpValue = 0.4f;
        [SerializeField]
        [Range(0, 5)]
        private float maxLookaheadDistanceX = 2.0f;
        [SerializeField]
        [Range(0, 5)]
        private float cameraMoveSpeed = 2.0f;


        private Vector2 currentCameraVelocity;
        private PlayerController playerController;

        void Start()
        {
            playerController = GlobalData.Instance.playerController; // cash out the reference to the player controller

           // Vector2 lastFramePlayerPosition = new Vector2(playerController.gameObject.transform.position.x, playerController.gameObject.transform.position.y); // make it a function
            currentCameraVelocity = Vector2.zero;
        }

        void Update()
        {/*
            
            Vector2 deltaTranslation = new Vector2(thisFramePlayerPosition.x, thisFramePlayerPosition.y);
            deltaTranslation -= lastFramePlayerPosition; // should already be scaled by the timeStep*/

            float lookaheadDistanceScaled = playerController.getDirectedInput() * maxLookaheadDistanceX;

            // Vector2 predictedPosition = new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.y);
            // predictedPosition += deltaTranslation;

            // read it from ground movement or some other interface?
            Vector2 thisFramePlayerPosition = new Vector2(playerController.gameObject.transform.position.x, playerController.gameObject.transform.position.y);
            Vector2 predictedPosition = thisFramePlayerPosition;
            predictedPosition.x += lookaheadDistanceScaled;

            Vector2 currrentCameraPosition = new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.y);
            Vector2 deltaPosition = predictedPosition;
            deltaPosition -= currrentCameraPosition;
            Vector2 rawCameraDirection = deltaPosition;
            rawCameraDirection.Normalize();
            rawCameraDirection *= cameraMoveSpeed;

            // currentCameraVelocity = Library.SmoothingFuncitons.LerpToReferenceNonLinear(currentCameraVelocity, rawCameraDirection, riseLerpValue, riseLerpValue);

            currrentCameraPosition = Vector2.Lerp(currrentCameraPosition, predictedPosition, playerController.getUndirectedInput());

            Debug.Log(currrentCameraPosition);

            this.gameObject.transform.position.Set(currrentCameraPosition.x, currrentCameraPosition.y, this.gameObject.transform.position.z);
        }
    }
}