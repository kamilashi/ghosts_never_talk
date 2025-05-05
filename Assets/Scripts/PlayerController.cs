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
        private float inputSensitivity; // setup in the explorer


        // setup internally:
        private CharacterMovement characterMovement;
        private PlayerRespawn playerRespawn;
        private SpriteRenderer spriteRenderer;
        private Animator animator;

        private MoveDirection lastMoveDirection;
        private float moveKeyHoldTimeScaled;
        [SerializeField] private bool acceptInput = true;
        [SerializeField] private bool processSplinePoints = true;

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
            characterMovement = gameObject.GetComponent<CharacterMovement>();
            playerRespawn = gameObject.GetComponent<PlayerRespawn>();
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            animator = gameObject.GetComponent<Animator>();
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
                    characterMovement.ResetMovement();
                    SetBlockInput(true);
                    currentAvailableTeleporter.Interact(this.transform, characterMovement);

                    bufferedTeleporter = currentAvailableTeleporter;
                    OnTeleportCamera();
                }
                else if (currentAvailableTrigger != null && Input.GetKeyDown(interactMappedKey))
                {
                    resetMovementInput();
                    characterMovement.ResetMovement();
                    SetBlockInput(true);
                    currentAvailableTrigger.Interact(this.transform, characterMovement, OnBlockInputDurationEnd);
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

            if(!processSplinePoints)
            {
                return;
            }

            {
                SplinePointObject availableObject = getAvailableSplinePointObject();

                processAvailableCheckpoint(availableObject);

                processAvailableTeleporter(availableObject);

                processAvailableTrigger(availableObject);
                
                processKillZone(availableObject);
            }

            if (characterMovement.GetCurrentGroundLayer().IsShiftedDown()) 
            {
                spriteRenderer.enabled = false;
                animator.speed = 0.0f;
            }
            else
            {
                spriteRenderer.enabled = true;
                animator.speed = 1.0f;
            }
        }

        private void processAvailableCheckpoint(SplinePointObject splineObject)
        {
            if (splineObject != null && splineObject.CanExecuteSplineObject() && splineObject.IsOfType(SplinePointObjectType.CheckPoint))
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
            InteractableTrigger newTrigger = null;
            if (splineObject != null && splineObject.IsOfType(SplinePointObjectType.InteractableTrigger))
            {
                newTrigger = (InteractableTrigger) splineObject;
                if (currentAvailableTrigger != newTrigger)
                {
                    currentAvailableTrigger?.OnBecomeUnavailable();

                    newTrigger.OnBecomeAvailable();
                    currentAvailableTrigger = newTrigger;
                }
            }

            // found no available triggers:
            if (newTrigger == null && currentAvailableTrigger != null)
            {
                currentAvailableTrigger.OnBecomeUnavailable();
                currentAvailableTrigger = null;
            }
        }

        private void processAvailableTeleporter(SplinePointObject splineObject)
        {
            InteractableTeleporter newTeleporter = null;

            if (splineObject != null && splineObject.IsOfType(SplinePointObjectType.InteractableTeleporter) && !((InteractableTeleporter)splineObject).isReceiverOnly())
            {
                newTeleporter = (InteractableTeleporter)splineObject;
                if (currentAvailableTeleporter != newTeleporter)
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
            
            // found no available teleporters:
            if (newTeleporter == null && currentAvailableTeleporter != null)
            {
                currentAvailableTeleporter.OnBecomeUnavailable();
                currentAvailableTeleporter = null;
            }
        }
        
        private void processKillZone(SplinePointObject splineObject)
        {
            if (splineObject != null && splineObject.CanExecuteSplineObject() && splineObject.IsOfType(SplinePointObjectType.KillZone))
            {
                TriggerRespawn();
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
            CameraMovement playerCameraMovement = GameManager.Instance.CameraMovementStaticRef;
            playerCameraMovement.SetPlayerFollowTeleportDampLambda();
            playerCameraMovement.Dolly(2.0f);
        }

        public void OnPlayerTeleportTranslateAnimationEvent()
        {
            bufferedTeleporter.Teleport(characterMovement); // is basically just calling groundMovement.teleportToPoint(idx, groundLayer)

            bufferedTeleporter.OnBecomeUnavailable();
            bufferedTeleporter = null;

            CameraMovement playerCameraMovement = GameManager.Instance.CameraMovementStaticRef;
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
        private void setParameterEnabled(ref bool parameter, bool isEnabled)
        {
            parameter = isEnabled;
        }
        private void resetMovementInput()
        {
            moveKeyHoldTimeScaled = 0.0f;
        }
        public void SetBlockInput(bool blockInputEnabled)
        {
            setParameterEnabled(ref acceptInput, !blockInputEnabled);
        }
        
        public void SetBlockSplinePointProcessing(bool blockProcessingEnabled)
        {
            setParameterEnabled(ref processSplinePoints, !blockProcessingEnabled);
        }

        public void BlockInputAnimationEvent(float duration)
        {
            // Create event handler delegate and pass it to the duration event constructor
            ProcessingHelpers.OnFinishedCallbackDelegate eventHandlerDelegate = OnBlockInputDurationEnd;
            GameManager.Instance.AnimationEventProcessorInstance.RegisterDurationEvent(duration, eventHandlerDelegate);
            SetBlockInput(true);
        }

        public void OnBlockInputDurationEnd()
        {
            SetBlockInput(false);
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

        [ContextMenu("TriggerRespawn")]
        void TriggerRespawn()
        {
            if (currentAvailableCheckPoint != null)
            {
                playerRespawn.StartRespawn();
            }
            else
            {
                Debug.LogError("No available check points!");
            }
        }
    }
}