using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                ProcessMoveInput(1.0f);
                groundMovement.SetMovementInput(MoveDirection.Left, MoveSpeed.Run);
                lastMoveDirection = MoveDirection.Left;
            }
            else if (Input.GetKey(moveRightMappedKey))
            {
                ProcessMoveInput(1.0f);
                groundMovement.SetMovementInput(MoveDirection.Right, MoveSpeed.Run);
                lastMoveDirection = MoveDirection.Right;
            }
            else if (Input.GetKeyUp(moveLeftMappedKey) || Input.GetKeyUp(moveRightMappedKey))
            {
                ProcessMoveInput(-1.0f);
                groundMovement.SetMovementInput(lastMoveDirection, MoveSpeed.Stand);
            }
            else
            {
                ProcessMoveInput(-1.0f);
            }

            Vector3 debugPos = new Vector3( 100.0f, 100.0f, 0.0f);
            Debug.DrawLine(debugPos, debugPos + Vector3.right * moveKeyHoldTimeScaled * 10.0f, Color.magenta, Time.deltaTime, false);
        }

       private void ProcessMoveInput(float sign)
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
    }

}