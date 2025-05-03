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

        private CharacterMovement characterMovement;
        private CharacterSteering characterSteering;
        private SpriteRenderer spriteRenderer;

        private MoveDirection lastMoveDirection;

        // later once we have interactables, this will need to move to that component
        private GroundLayerPositionMapper groundLayerPositionMapper;

        [SerializeField] private float moveKeyHoldTimeScaled;
        private bool acceptInput = true;


        [SerializeField] private InteractableTeleporter currentAvailableTeleporter;
        private InteractableTeleporter bufferedTeleporter;

        [SerializeField] private InteractableTrigger currentAvailableTrigger;
        [SerializeField] private CheckPoint currentAvailableCheckPoint;

        // #Todo: get this data from control map
        KeyCode moveLeftMappedKey = KeyCode.A;
        KeyCode moveRightMappedKey = KeyCode.D;
        
        KeyCode advanceDialogueMappedKey = KeyCode.Space;

        KeyCode interactMappedKey = KeyCode.F;

        void Awake()
        {
            lastMoveDirection = MoveDirection.Right;
            moveKeyHoldTimeScaled = 0.0f;
            characterMovement = gameObject.GetComponentInChildren<CharacterMovement>();
            characterSteering = gameObject.GetComponentInChildren<CharacterSteering>();
            groundLayerPositionMapper = gameObject.GetComponentInChildren<GroundLayerPositionMapper>();
            spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
            bufferedTeleporter = null;
            currentAvailableTrigger = null;
            currentAvailableCheckPoint = null;
        }   
        
        void Start()
        {

        }

        void Update()
        {
            if(acceptInput && !GameManager.Instance.DialogueRunnerStaticRef.IsDialogueRunning)
            { 
                if (Input.GetKey(moveLeftMappedKey))
                {
                    processMoveInput(1.0f);
                    lastMoveDirection = characterMovement.IsTurning() ? lastMoveDirection : MoveDirection.Left;
                characterMovement.SetMovementInput(lastMoveDirection, MoveSpeed.Run);
                }
                else if (Input.GetKey(moveRightMappedKey))
                {
                    processMoveInput(1.0f);
                    lastMoveDirection = characterMovement.IsTurning() ? lastMoveDirection : MoveDirection.Right;
                characterMovement.SetMovementInput(lastMoveDirection, MoveSpeed.Run);
                }
                else
                {
                    processMoveInput(-1.0f);
                    characterMovement.SetMovementInput(lastMoveDirection, MoveSpeed.Stand);
                }

                if (currentAvailableTeleporter != null &&  Input.GetKeyDown(interactMappedKey))
                {
                    resetMovementInput();
                    currentAvailableTeleporter.Interact(this.transform, characterMovement);

                    // player specific:
                    bufferedTeleporter = currentAvailableTeleporter;
                    OnTeleportCamera();
                }
                else if (currentAvailableTrigger != null && Input.GetKeyDown(interactMappedKey))
                {
                    resetMovementInput();
                    currentAvailableTrigger.Interact(this.transform, characterMovement);
                }
            }
            else
            {
                resetMovementInput();
            }

            if (acceptInput && Input.GetKeyDown(advanceDialogueMappedKey))
            {
                GameManager.Instance.DialogueViewStaticRef.UserRequestedViewAdvancement();
            }

            {
                SplinePointObject availableObject = getAvailableSplinePointObject();

                processAvailableCheckpoint(availableObject);

                processAvailableTeleporter(availableObject);

                processAvailableTrigger(availableObject);
            }
        }

        private void processAvailableCheckpoint(SplinePointObject splineObject)
        {
            if (splineObject != null && splineObject.IsOfType(SplinePointObjectType.CheckPoint))
            {
                CheckPoint newCheckPoint = (CheckPoint)splineObject;
                if (newCheckPoint != currentAvailableCheckPoint)
                {
                    //currentAvailableCheckPoint?.OnBecomeUnavailable();
                    newCheckPoint.OnBecomeAvailable();
                    currentAvailableCheckPoint = newCheckPoint;
                }
            }
        }

        private void processAvailableTrigger(SplinePointObject splineObject)
        {
            if (splineObject != null && splineObject.IsOfType(SplinePointObjectType.InteractableTrigger))
            {
                InteractableTrigger newTrigger = (InteractableTrigger) splineObject;
                if (currentAvailableTrigger != newTrigger)
                {
                    currentAvailableTrigger?.OnBecomeUnavailable();

                    newTrigger.OnBecomeAvailable();
                    currentAvailableTrigger = newTrigger;
                }
            }
            else if (currentAvailableTrigger != null)
            {
                currentAvailableTrigger.OnBecomeUnavailable();
                currentAvailableTrigger = null;
            }
        }

        private void processAvailableTeleporter(SplinePointObject splineObject)
        {
            if (splineObject != null && splineObject.IsOfType(SplinePointObjectType.InteractableTeleporter))
            {
                InteractableTeleporter newTeleporter = (InteractableTeleporter)splineObject;
                if (currentAvailableTeleporter != newTeleporter && !newTeleporter.isReceiverOnly())
                {
                    currentAvailableTeleporter?.OnBecomeUnavailable();
                    newTeleporter.OnBecomeAvailable();
                    currentAvailableTeleporter = newTeleporter;

                    if (currentAvailableTrigger != null)
                    {
                        currentAvailableTrigger.OnBecomeUnavailable();
                        currentAvailableTrigger = null;
                    }
                }
            }
            else if (currentAvailableTeleporter != null)
            {
                currentAvailableTeleporter.OnBecomeUnavailable();
                currentAvailableTeleporter = null;
            }
        }


        private void processMoveInput(float sign)
        {
            moveKeyHoldTimeScaled += sign * Time.deltaTime * inputSensitivity; // replace with smoothing curves? 
            moveKeyHoldTimeScaled = Mathf.Clamp01(moveKeyHoldTimeScaled);
        }

        private SplinePointObject getAvailableSplinePointObject()
        {
            return characterMovement.GetAvailableSplinePointObject();
        }

        private void OnTeleportCamera()
        {
            // the animation should come from the teleporting interactable!
            CameraMovement playerCameraMovement = GameManager.Instance.GetActiveCamera().GetComponent<CameraMovement>();
            playerCameraMovement.SetPlayerFollowTeleportDampLambda();
            playerCameraMovement.Dolly(2.0f);
        }

        public void OnPlayerTeleportTranslateAnimationEvent()
        {
            bufferedTeleporter.Teleport(characterMovement); // is basically just calling groundMovement.teleportToPoint(idx, groundLayer)

            bufferedTeleporter.OnBecomeUnavailable();
            bufferedTeleporter = null;

            CameraMovement playerCameraMovement = GameManager.Instance.GetActiveCamera().GetComponent<CameraMovement>();
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
        public void SetBlockInput(bool blockInputEnabled)
        {
            setAcceptInput(!blockInputEnabled);
        }

        public void BlockInputAnimationEvent(float duration)
        {
            // Create event handler delegate and pass it to the duration event constructor
            ProcessingHelpers.OnFinishedCallbackDelegate eventHandlerDelegate = OnBlockInputDurationEnd;
            GameManager.Instance.AnimationEventProcessorInstance.RegisterDurationEvent(duration, eventHandlerDelegate);
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

        public void SetAvailableCheckPoint(CheckPoint checkPoint)
        {
            currentAvailableCheckPoint = checkPoint;
        }

        public CheckPoint GetAvailableCheckPoint()
        {
            return currentAvailableCheckPoint;
        }

        private void setCameraPlayerFollowEnabled(bool isEnabled)
        {
            GameManager.Instance.GetActiveCamera().GetComponent<CameraMovement>().SetPlayerFollowEnabled(isEnabled);
        }

        public string GetInteractKey()
        {
            return interactMappedKey.ToString();
        }
        public string GetAdvanceDialogueKey()
        {
            return advanceDialogueMappedKey.ToString();
        }


        [ContextMenu("RespawnTest")]
        void RespawnTest()
        {
            if (currentAvailableCheckPoint != null)
            {
                //groundMovement.SwitchToLayer(currentAvailableCheckPoint.ContainingGroundLayer);
                currentAvailableCheckPoint.Respawn(this, characterMovement, characterSteering);
            }
        }
    }
}