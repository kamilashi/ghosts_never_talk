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
        private SpriteRenderer spriteRenderer;

        // later once we have interactables, this will need to move to that component
        private GroundLayerPositionMapper groundLayerPositionMapper;

        [SerializeField] private float moveKeyHoldTimeScaled;
        private bool acceptInput = true;


        [SerializeField] private InteractableTeleporter currentAvailableTeleporter;
        private InteractableTeleporter bufferedTeleporter;


        [SerializeField] private InteractableTrigger currentAvailableTrigger;

        // #Todo: get this data from control map
        KeyCode moveLeftMappedKey = KeyCode.A;
        KeyCode moveRightMappedKey = KeyCode.D;
        
        KeyCode advanceDialogueMappedKey = KeyCode.Space;

        KeyCode interactMappedKey = KeyCode.F;

        void Awake()
        {
            lastMoveDirection = MoveDirection.Right;
            moveKeyHoldTimeScaled = 0.0f;
            groundMovement = gameObject.GetComponentInChildren<GroundMovement>();
            groundLayerPositionMapper = gameObject.GetComponentInChildren<GroundLayerPositionMapper>();
            spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
            bufferedTeleporter = null;
        }   
        
        void Start()
        {

        }

        void Update()
        {
            if(acceptInput && !GlobalData.Instance.DialogueRunnerStaticRef.IsDialogueRunning)
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
                /*else if (Input.GetKeyUp(moveLeftMappedKey) || Input.GetKeyUp(moveRightMappedKey))
                {
                    processMoveInput(-1.0f);
                    groundMovement.SetMovementInput(lastMoveDirection, MoveSpeed.Stand);
                }*/
                else
                {
                    processMoveInput(-1.0f);
                    groundMovement.SetMovementInput(lastMoveDirection, MoveSpeed.Stand);
                }

                if (currentAvailableTeleporter != null &&  Input.GetKeyDown(interactMappedKey))
                {
                    resetMovementInput();
                    currentAvailableTeleporter.Interact(this.transform, groundMovement);
                    bufferedTeleporter = currentAvailableTeleporter;
                    OnTeleportCamera();
                }
                else if (currentAvailableTrigger != null && Input.GetKeyDown(interactMappedKey))
                {
                    resetMovementInput();
                    currentAvailableTrigger.Interact(this.transform, groundMovement);
                }
            }
            else
            {
                resetMovementInput();
            }

            if (acceptInput && Input.GetKeyDown(advanceDialogueMappedKey))
            {
                GlobalData.Instance.DialogueViewStaticRef.UserRequestedViewAdvancement();
            }

            {
                processAvailableTeleporters();

                processAvailableInteractions();
            }
        }

        private void processAvailableTeleporters()
        {
            InteractableTeleporter availableTeleporter = getClosestTeleporter();

            if (availableTeleporter != null)
            {
                if (currentAvailableTeleporter != availableTeleporter)
                {
                    availableTeleporter.OnBecomeAvailable();
                    currentAvailableTeleporter = availableTeleporter;
                }
            }
            else if (currentAvailableTeleporter != null)
            {
                currentAvailableTeleporter.OnBecomeUnavailable();
                currentAvailableTeleporter = null;
            }

        }
        
        private void processAvailableInteractions()
        {
            InteractableTrigger availableTrigger = getClosestInteractableTrigger();
            if (availableTrigger != null)
            {
                if (currentAvailableTrigger != availableTrigger)
                {
                    //Debug.Log("Trigger available");
                    availableTrigger.OnBecomeAvailable();
                    currentAvailableTrigger = availableTrigger;
                }
            }
            else if (currentAvailableTrigger != null)
            {
                //Debug.Log("Trigger UNavailable");
                currentAvailableTrigger.OnBecomeUnavailable();
                currentAvailableTrigger = null;
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
            foreach (InteractableTeleporter teleporter in GlobalData.Instance.ActiveSceneDynamicRef.GetPlayerVisibleTeleporters())
            {
                if (teleporter.IsInRangeX(this.transform.position))
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
                if (interactableTrigger.IsInRangeX(this.transform.position))
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
            GlobalData.Instance.ActiveSceneDynamicRef.SwitchToLayer(bufferedTeleporter.TargetTeleporter.ContainingGroundLayer.GroundLayerIndex);

            //important: run this function AFTER changing the current layer. Perhabs it's better to keep track of the current active layer in the controller instead of the global data. This way enemies could make use of their own at some point.
            bufferedTeleporter.Teleport(spriteRenderer, groundMovement);

            bufferedTeleporter.gameObject.GetComponent<VfxPlayer>().PlayVfxExit();
            bufferedTeleporter = null;

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
        private void resetMovementInput()
        {
            moveKeyHoldTimeScaled = 0.0f;
        }

        public void BlockInputAnimationEvent(float duration)
        {
            // Create event handler delegate and pass it to the duration event constructor
            ProcessingHelpers.OnFinishedCallbackDelegate eventHandlerDelegate = OnBlockInputDurationEnd;
            GlobalData.Instance.AnimationEventProcessorInstance.RegisterDurationEvent(duration, eventHandlerDelegate);
            setAcceptInput(false);
        }

        public void OnBlockInputDurationEnd()
        {
            setAcceptInput(true);
        }

        public Interactable GetAvailableInteractable()
        {
            return currentAvailableTeleporter != null ? currentAvailableTeleporter : currentAvailableTrigger;
        }

        private void setCameraPlayerFollowEnabled(bool isEnabled)
        {
            GlobalData.Instance.GetActiveCamera().GetComponent<CameraMovement>().SetPlayerFollowEnabled(isEnabled);
        }

        public string GetInteractKey()
        {
            return interactMappedKey.ToString();
        }
        public string GetAdvanceDialogueKey()
        {
            return advanceDialogueMappedKey.ToString();
        }
    }

}