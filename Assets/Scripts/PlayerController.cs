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

        private float moveKeyHoldTimeScaled;
        private bool acceptInput = true;

        void Awake()
        {
            lastMoveDirection = MoveDirection.Right;
            moveKeyHoldTimeScaled = 0.0f;
            groundMovement = gameObject.GetComponentInChildren<GroundMovement>();
        }   
        
        void Start()
        {

        }

        void Update()
        {
                // #Todo: get this data from control map
                KeyCode moveLeftMappedKey = KeyCode.A;
                KeyCode moveRightMappedKey = KeyCode.D;

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
                /* Vector3 debugPos = new Vector3( 100.0f, 100.0f, 0.0f);
                 Debug.DrawLine(debugPos, debugPos + Vector3.right * moveKeyHoldTimeScaled * 10.0f, Color.magenta, Time.deltaTime, false);*/
        }

       private void processMoveInput(float sign)
        {
            moveKeyHoldTimeScaled += sign * Time.deltaTime * inputSensitivity; // replace with smoothing curves? 
            moveKeyHoldTimeScaled = Mathf.Clamp01(moveKeyHoldTimeScaled);
        }

        public int GetLastDirectionInput()
        {
            return (int)lastMoveDirection;
        }

        public float GetMoveKeyHoldScale()
        {
            return moveKeyHoldTimeScaled;
        }

        public void BlockInputAnimationEvent(float duration)
        {
            if (duration > 0)
            {
                // Create event handler delegate and pass it to the duration event constructor
                ProcessingHelpers.OnFinishedCallbackDelegate eventHandlerDelegate = onBlockInputDurationEnd;
                GlobalData.Instance.animationEventProcessor.RegisterDurationEvent(duration, eventHandlerDelegate);
                setAcceptInput(false);
            }
            else
            {
                Assert.IsTrue(true, "Duration events must have a duration!");
                // #todo: exception?
            }
        }

        private void setAcceptInput(bool isEnabled)
        {
            acceptInput = isEnabled;
        }

        private void onBlockInputDurationEnd()
        {
            setAcceptInput(true);
        }
    }

}