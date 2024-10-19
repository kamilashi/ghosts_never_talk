using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    public class CameraMovement : MonoBehaviour
    {
        [SerializeField]
        [Range(0, 10)]
        private float directionChangeReactionSpeed = 5.0f;
        [SerializeField]
        [Range(0, 5)]
        private float maxLookaheadDistanceX = 2.0f;

        private PlayerController playerController;


        private float directionSmoothed = 0.0f;

        void Start()
        {
            playerController = GlobalData.Instance.playerController; // cash out the reference to the player controllers
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

            Vector2 currrentCameraPosition = new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.y);
            Vector2 deltaPosition = predictedPosition;
            deltaPosition -= currrentCameraPosition;

            gameObject.transform.Translate(deltaPosition);
        }
    }
}