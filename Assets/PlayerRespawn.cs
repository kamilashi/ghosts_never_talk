using GNT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public enum RespawnStateMachine
    {
        Inactive,
        PlayingRespawnAnimation,
        Moving,
        PlayingSpawnAnimation
    }

    [SerializeField] private RespawnStateMachine currentState;
    [SerializeField] private CharacterMovement playerMovement;
    [SerializeField] private CharacterSteering playerSteering;
    [SerializeField] private PlayerController playerController;

    // Start is called before the first frame update
    void Awake()
    {
        currentState = RespawnStateMachine.Inactive;
        playerController = GetComponent<PlayerController>();
        playerSteering = GetComponent<CharacterSteering>();
        playerMovement = GetComponent<CharacterMovement>();
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
                        currentState = RespawnStateMachine.Inactive;
                    }
                    break;
                }
        }
    }


    public void Respawn(CheckPoint checkPoint)
    {
        currentState = RespawnStateMachine.PlayingRespawnAnimation;

        playerSteering.StartSteeringOnPath(checkPoint.GetSplinePointIndex(), checkPoint.ContainingGroundLayer.MovementSpline, 5.0f);

        playerMovement.StopAndPlayAnimation(playerMovement.RespawnAnimation);
        playerController.SetBlockInput(true);
    }
}
