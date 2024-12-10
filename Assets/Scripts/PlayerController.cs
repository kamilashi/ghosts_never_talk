using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace GNT
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        [Range(0, 1000)]
        private float inputSensitivity;

        private GroundMovement groundMovement;
        private MoveDirection lastMoveDirection;

        // later once we have interactables, this will need to move to that component
        private GroundLayerPositionMapper groundLayerPositionMapper;

        [SerializeField]
        private float moveKeyHoldTimeScaled;
        private bool acceptInput = true;
        private LayerSwitchDirection bufferedLayerSwitchDirection;

        void Awake()
        {
            lastMoveDirection = MoveDirection.Right;
            moveKeyHoldTimeScaled = 0.0f;
            groundMovement = gameObject.GetComponentInChildren<GroundMovement>();
            groundLayerPositionMapper = gameObject.GetComponentInChildren<GroundLayerPositionMapper>();
        }   
        
        void Start()
        {

        }

        void Update()
        {
            if(acceptInput)
            { 
                // #Todo: get this data from control map
                KeyCode moveLeftMappedKey = KeyCode.A;
                KeyCode moveRightMappedKey = KeyCode.D;

                KeyCode switchGroundLayerIn = KeyCode.W;
                KeyCode switchGroundLayerOut = KeyCode.S;

                if (Input.GetKey(moveLeftMappedKey))
                {
                    processMoveInput(1.0f);
                    lastMoveDirection = groundMovement.IsTurning() ? lastMoveDirection : MoveDirection.Left;
                groundMovement.SetMovementInput(lastMoveDirection, MoveSpeed.Run);
                }
                else if (Input.GetKey(moveRightMappedKey))
                {
                    processMoveInput(1.0f);
                    lastMoveDirection = groundMovement.IsTurning() ? lastMoveDirection : MoveDirection.Right;
                groundMovement.SetMovementInput(lastMoveDirection, MoveSpeed.Run);
                }
                else if (Input.GetKeyUp(moveLeftMappedKey) || Input.GetKeyUp(moveRightMappedKey))
                {
                    processMoveInput(-1.0f);
                    groundMovement.SetMovementInput(lastMoveDirection, MoveSpeed.Stand);
                }
                else
                {
                    processMoveInput(-1.0f);
                }

                if (Input.GetKeyDown(switchGroundLayerIn))
                {
                    bufferedLayerSwitchDirection = LayerSwitchDirection.In;
                    initiatePlayerTeleport();
                }
                else if (Input.GetKeyDown(switchGroundLayerOut))
                {
                    bufferedLayerSwitchDirection = LayerSwitchDirection.Out;
                    initiatePlayerTeleport();
                }
            }
        }

       private void processMoveInput(float sign)
        {
            moveKeyHoldTimeScaled += sign * Time.deltaTime * inputSensitivity; // replace with smoothing curves? 
            moveKeyHoldTimeScaled = Mathf.Clamp01(moveKeyHoldTimeScaled);
        }
        
        private void initiatePlayerTeleport()
        {
            // the animation should come from the teleporting interactable!
            groundMovement.InitiateTeleportWithAnimation(groundMovement.teleportAnimation);
            CameraMovement playerCameraMovement = GlobalData.Instance.GetActiveCamera().GetComponent<CameraMovement>();
            playerCameraMovement.SetPlayerFollowTeleportDampLambda();
            playerCameraMovement.Dolly(2.0f);
        }

        public void OnPlayerTeleportTranslateAnimationEvent()
        {
            // the switch direction (more like the target layer link) should come from the teleporting interactable instead!
            if (bufferedLayerSwitchDirection == LayerSwitchDirection.In)
            {
                GlobalData.Instance.ActiveScene.SwitchIn();
            }
            else
            {
                GlobalData.Instance.ActiveScene.SwitchOut();
            }

            Vector3 teleportDelta = Vector3.zero;
            groundLayerPositionMapper.TeteportToGroundHookPosition(ref teleportDelta, groundMovement.GroundCollisionMask, groundMovement.GetCollider());
            groundMovement.TeleportTranslateToLayer(teleportDelta, GlobalData.Instance.ActiveScene.ActiveGroundLayer.SpriteLayerOrder);
            CameraMovement playerCameraMovement = GlobalData.Instance.GetActiveCamera().GetComponent<CameraMovement>();
            playerCameraMovement.ResetDolly();
        }

        public int GetLastDirectionInput()
        {
            return (int)lastMoveDirection;
        }
        public float GetMoveKeyHoldScale()
        {
            return moveKeyHoldTimeScaled;
        }
        private void setAcceptInput(bool isEnabled)
        {
            acceptInput = isEnabled;
        }

        public void BlockInputAnimationEvent(float duration)
        {
            // Create event handler delegate and pass it to the duration event constructor
            ProcessingHelpers.OnFinishedCallbackDelegate eventHandlerDelegate = OnBlockInputDurationEnd;
            GlobalData.Instance.animationEventProcessor.RegisterDurationEvent(duration, eventHandlerDelegate);
            setAcceptInput(false);
            moveKeyHoldTimeScaled = 0.0f;
        }

        public void OnBlockInputDurationEnd()
        {
            setAcceptInput(true);
        }

        private void setCameraPlayerFollowEnabled(bool isEnabled)
        {
            GlobalData.Instance.GetActiveCamera().GetComponent<CameraMovement>().SetPlayerFollowEnabled(isEnabled);
        }
    }

}