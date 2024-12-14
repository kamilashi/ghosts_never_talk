using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace GNT
{
    public class PlayerController : MonoBehaviour
    {
        public int AvailableActionUIKey = -1;

        [SerializeField]
        [Range(0, 1000)]
        private float inputSensitivity;

        private GroundMovement groundMovement;
        private MoveDirection lastMoveDirection;
        private SpriteRenderer spriteRenderer;

        // later once we have interactables, this will need to move to that component
        private GroundLayerPositionMapper groundLayerPositionMapper;

        [SerializeField]
        private float moveKeyHoldTimeScaled;
        private bool acceptInput = true;
        private InteractableTeleporter bufferedInteractableTeleporter;

        void Awake()
        {
            lastMoveDirection = MoveDirection.Right;
            moveKeyHoldTimeScaled = 0.0f;
            groundMovement = gameObject.GetComponentInChildren<GroundMovement>();
            groundLayerPositionMapper = gameObject.GetComponentInChildren<GroundLayerPositionMapper>();
            spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
            bufferedInteractableTeleporter = null;
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
               
                KeyCode interactKey = KeyCode.F;

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
                    groundMovement.SetMovementInput(lastMoveDirection, MoveSpeed.Stand);
                }

/*
                if (Input.GetKeyDown(switchGroundLayerIn))
                {
                    bufferedLayerSwitchDirection = LayerSwitchDirection.In;
                    initiatePlayerTeleport();
                }
                else if (Input.GetKeyDown(switchGroundLayerOut))
                {
                    bufferedLayerSwitchDirection = LayerSwitchDirection.Out;
                    initiatePlayerTeleport();
                }*/

                InteractableTeleporter availableTeleporter = getClosestTeleporter();
                if(availableTeleporter != null)
                {
                    if(Input.GetKeyDown(interactKey))
                    {
                        availableTeleporter.Interact(this.transform, groundMovement);
                        bufferedInteractableTeleporter = availableTeleporter;
                        OnTeleportCamera();
                    }
                }
            }
        }

       private void processMoveInput(float sign)
        {
            moveKeyHoldTimeScaled += sign * Time.deltaTime * inputSensitivity; // replace with smoothing curves? 
            moveKeyHoldTimeScaled = Mathf.Clamp01(moveKeyHoldTimeScaled);
        }

        private InteractableTeleporter getClosestTeleporter()
        {
            // we assume that there will not be closely placed teleporters in levels!
            foreach (InteractableTeleporter teleporter in GlobalData.Instance.ActiveScene.GetPlayerVisibleTeleporters())
            {
                if (teleporter.IsInRange(this.transform.position))
                {
                    return teleporter;
                }
            }

            return null;
        }
        private void OnTeleportCamera()
        {
            // the animation should come from the teleporting interactable!
            CameraMovement playerCameraMovement = GlobalData.Instance.GetActiveCamera().GetComponent<CameraMovement>();
            playerCameraMovement.SetPlayerFollowTeleportDampLambda();
            playerCameraMovement.Dolly(2.0f);
        }

        public void OnPlayerTeleportTranslateAnimationEvent()
        {
            GlobalData.Instance.ActiveScene.SwitchToLayer(bufferedInteractableTeleporter.TargetTeleporter.ContainingGroundLayer.GroundLayerIndex);
            Vector3 deltaTeleport = bufferedInteractableTeleporter.TeteportToTargetPosition(transform, groundMovement.GroundCollisionMask, groundMovement.GetCollider(), spriteRenderer);
            transform.Translate(deltaTeleport, Space.World);
            bufferedInteractableTeleporter = null;

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