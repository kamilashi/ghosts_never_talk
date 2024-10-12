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
        private float moveKeyHoldTime;

        void Awake()
        {
            lastMoveDirection = MoveDirection.Right;
            moveKeyHoldTime = 0.0f;
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
                groundMovement.SetMovementInput(MoveDirection.Left, MoveSpeed.Run);
                lastMoveDirection = MoveDirection.Left;
            }
            else if (Input.GetKey(moveRightMappedKey))
            {
                processMoveInput(1.0f);
                groundMovement.SetMovementInput(MoveDirection.Right, MoveSpeed.Run);
                lastMoveDirection = MoveDirection.Right;
            }
            else if (Input.GetKeyUp(moveLeftMappedKey) || Input.GetKeyUp(moveRightMappedKey))
            {
                processMoveInput(-1.0f);
                groundMovement.SetMovementInput(lastMoveDirection, MoveSpeed.Stand);
            }
        }

       private void processMoveInput(float sign)
        {
            moveKeyHoldTime += sign * Time.deltaTime * inputSensitivity; // replace with smoothing curves? 
            moveKeyHoldTime = Mathf.Clamp01(moveKeyHoldTime);
        }
    }

}