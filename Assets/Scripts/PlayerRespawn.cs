using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    public class PlayerRespawn : MonoBehaviour
    {
        public enum RespawnStateMachine
        {
            Inactive,
            PlayingRespawnAnimation,
            Moving,
            PlayingSpawnAnimation
        }

        public float MaxMovementSpeed;

        [SerializeField] private RespawnStateMachine currentState;
        private CharacterMovement playerMovement;
        private CharacterSteering playerSteering;
        private PlayerController playerController;

        // Start is called before the first frame update
        void Awake()
        {
            currentState = RespawnStateMachine.Inactive;
            playerController = GetComponent<PlayerController>();
            playerSteering = GetComponent<CharacterSteering>();
            playerMovement = GetComponent<CharacterMovement>();

            Debug.Assert(MaxMovementSpeed != 0.0f, "MaxMovementSpeed should be non-zero!");
        }

        // Update is called once per frame
        void Update()
        {
            switch (currentState)
            {
                case RespawnStateMachine.Inactive:
                    {
                        break;
                    }
                case RespawnStateMachine.PlayingRespawnAnimation:
                    {
                        if (playerMovement.IsAnimationFinished(playerMovement.RespawnAnimation))
                        {
                            CheckPoint targetCheckPoint = playerController.GetAvailableCheckPoint();
                            playerSteering.StartSteeringOnPath(targetCheckPoint.GetSplinePointIndex(), targetCheckPoint.ContainingGroundLayer.MovementSpline, MaxMovementSpeed);
                            currentState = RespawnStateMachine.Moving;
                        }
                        break;
                    }
                case RespawnStateMachine.Moving:
                    {
                        if (playerSteering.HasArrived())
                        {
                            playerMovement.StopAndPlayAnimation(playerMovement.SpawnAnimation);
                            currentState = RespawnStateMachine.PlayingSpawnAnimation;
                        }
                        break;
                    }
                case RespawnStateMachine.PlayingSpawnAnimation:
                    {
                        if (playerMovement.IsAnimationFinished(playerMovement.SpawnAnimation))
                        {
                            playerController.SetBlockInput(false);
                            playerController.SetBlockSplinePointProcessing(false);
                            currentState = RespawnStateMachine.Inactive;
                        }
                        break;
                    }
            }
        }


        public void StartRespawn()
        {
            currentState = RespawnStateMachine.PlayingRespawnAnimation;

            playerMovement.StopAndPlayAnimation(playerMovement.RespawnAnimation);
            playerController.SetBlockInput(true);
            playerController.SetBlockSplinePointProcessing(true);
        }
    }

}