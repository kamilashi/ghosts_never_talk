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
                    GlobalData.Instance.ActiveScene.SwitchIn();
                    hackySnapToGroundLayerHook();
                }
                else if(Input.GetKeyDown(switchGroundLayerOut))
                {
                    GlobalData.Instance.ActiveScene.SwitchOut();
                    hackySnapToGroundLayerHook();
                }
            }
        }

       private void processMoveInput(float sign)
        {
            moveKeyHoldTimeScaled += sign * Time.deltaTime * inputSensitivity; // replace with smoothing curves? 
            moveKeyHoldTimeScaled = Mathf.Clamp01(moveKeyHoldTimeScaled);
        }
        
        private void hackySnapToGroundLayerHook()
        {
            float teleportDistanceSquare = 0.0f;
            groundLayerPositionMapper.TeteportToGroundHookPosition(ref teleportDistanceSquare);
            setCameraPlayerFollowEnabled(false);
            groundMovement.SetFreezeMovement(true);
            GlobalData.Instance.GetActiveCamera().GetComponent<CameraMovement>().SetPlayerFollowTeleportDampLambda();
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
        }

        public void OnBlockInputDurationEnd()
        {
            setAcceptInput(true);
        }

        public void DisableCameraFollowAnimationEvent(float duration)
        {
            // Create event handler delegate and pass it to the duration event constructor
            ProcessingHelpers.OnFinishedCallbackDelegate eventHandlerDelegate = OnDisableCameraFollowDurationEnd;
            GlobalData.Instance.animationEventProcessor.RegisterDurationEvent(duration, eventHandlerDelegate);
            setCameraPlayerFollowEnabled(false);
        }

        public void OnDisableCameraFollowDurationEnd()
        {
            setCameraPlayerFollowEnabled(true);
        }

        private void setCameraPlayerFollowEnabled(bool isEnabled)
        {
            GlobalData.Instance.GetActiveCamera().GetComponent<CameraMovement>().SetPlayerFollowEnabled(isEnabled);
        }
    }

}