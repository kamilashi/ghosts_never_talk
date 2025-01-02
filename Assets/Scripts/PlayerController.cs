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

        private float moveKeyHoldTimeScaled;
        private bool acceptInput = true;

        private InteractableTeleporter availableInteractableTeleporter;
        private InteractableTeleporter bufferedInteractableTeleporter;

        private InteractableTrigger availableInteractableTrigger;

        // #Todo: get this data from control map
        KeyCode moveLeftMappedKey = KeyCode.A;
        KeyCode moveRightMappedKey = KeyCode.D;
        
        KeyCode progressDialogueMappedKey = KeyCode.Space;

        KeyCode interactKey = KeyCode.F;

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

                if(Input.GetKeyDown(progressDialogueMappedKey))
                {
                    GlobalData.Instance.DialogueViewStaticRef.UserRequestedViewAdvancement();
                }

                processAvailableInteractions(interactKey);

                //#ToDo: refactor this
                if(availableInteractableTeleporter == null)
                {
                    InteractableTrigger availableTrigger = getClosestInteractableTrigger();
                    if (availableTrigger != null)
                    {
                        if (Input.GetKeyDown(interactKey))
                        {
                            availableInteractableTrigger.Interact(this.transform, groundMovement);
                        }

                        if (availableInteractableTrigger != availableTrigger)
                        {
                            availableTrigger.TransformAnimateEnter();
                            availableTrigger.gameObject.GetComponent<VfxPlayer>().PlayVfxEnter(availableTrigger.ContainingGroundLayer.SpriteLayerOrder, availableTrigger.InteractRadius * 2.0f);
                        }
                    }
                    else if (availableInteractableTrigger != null)
                    {
                        availableInteractableTrigger.TransformAnimateExit();
                        availableInteractableTrigger.gameObject.GetComponent<VfxPlayer>().PlayVfxExit();
                    }
                    availableInteractableTrigger = availableTrigger;
                }
                else if(availableInteractableTrigger != null)
                {
                    availableInteractableTrigger.TransformAnimateExit();
                    availableInteractableTrigger.gameObject.GetComponent<VfxPlayer>().PlayVfxExit();
                    availableInteractableTrigger = null;
                }
            }
        }

        private void processAvailableInteractions(KeyCode interactKey)
        {
            InteractableTeleporter availableTeleporter = getClosestTeleporter();
            if (availableTeleporter != null)
            {
                if (Input.GetKeyDown(interactKey))
                {
                    availableTeleporter.Interact(this.transform, groundMovement);
                    bufferedInteractableTeleporter = availableTeleporter;
                    OnTeleportCamera();
                }

                if (availableInteractableTeleporter != availableTeleporter)
                {
                    availableTeleporter.gameObject.GetComponent<VfxPlayer>().PlayVfxEnter(availableTeleporter.ContainingGroundLayer.SpriteLayerOrder, availableTeleporter.InteractRadius * 2.0f);
                }
            }
            else if (availableInteractableTeleporter != null)
            {
                availableInteractableTeleporter.gameObject.GetComponent<VfxPlayer>().PlayVfxExit();
            }

            availableInteractableTeleporter = availableTeleporter;
        }

       private void processMoveInput(float sign)
        {
            moveKeyHoldTimeScaled += sign * Time.deltaTime * inputSensitivity; // replace with smoothing curves? 
            moveKeyHoldTimeScaled = Mathf.Clamp01(moveKeyHoldTimeScaled);
        }

        private InteractableTeleporter getClosestTeleporter()
        {
            // we assume that there will not be closely placed teleporters in levels!
            foreach (InteractableTeleporter teleporter in GlobalData.Instance.ActiveSceneDynamicRef.GetPlayerVisibleTeleporters())
            {
                if (teleporter.IsInRange(this.transform.position))
                {
                    return teleporter;
                }
            }

            return null;
        }
        
        private InteractableTrigger getClosestInteractableTrigger()
        {
            // we assume that there will not be closely placed teleporters in levels!
            foreach (InteractableTrigger interactableTrigger in GlobalData.Instance.ActiveSceneDynamicRef.GetPlayerVisibleInteractableTriggers())
            {
                if (interactableTrigger.IsInRange(this.transform.position))
                {
                    return interactableTrigger;
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
            GlobalData.Instance.ActiveSceneDynamicRef.SwitchToLayer(bufferedInteractableTeleporter.TargetTeleporter.ContainingGroundLayer.GroundLayerIndex);
            Vector3 deltaTeleport = bufferedInteractableTeleporter.TeteportToTargetPosition(transform, groundMovement.GroundCollisionMask, groundMovement.GetCollider(), spriteRenderer);
            transform.Translate(deltaTeleport, Space.World);

            bufferedInteractableTeleporter.gameObject.GetComponent<VfxPlayer>().PlayVfxExit();
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
            GlobalData.Instance.AnimationEventProcessorInstance.RegisterDurationEvent(duration, eventHandlerDelegate);
            setAcceptInput(false);
            moveKeyHoldTimeScaled = 0.0f;
        }

        public void OnBlockInputDurationEnd()
        {
            setAcceptInput(true);
        }

        public Interactable GetAvailableInteractable()
        {
            return availableInteractableTeleporter != null ? availableInteractableTeleporter : availableInteractableTrigger;
        }

        private void setCameraPlayerFollowEnabled(bool isEnabled)
        {
            GlobalData.Instance.GetActiveCamera().GetComponent<CameraMovement>().SetPlayerFollowEnabled(isEnabled);
        }

        public string GetInteractKey()
        {
            return interactKey.ToString();
        }
    }

}