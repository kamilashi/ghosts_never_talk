using GNT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CheckPoint : SplinePointObject
{
    public enum RespawnStateMachine
    {
        Inactive,
        PlayingRespawnAnimation,
        Moving,
        PlayingSpawnAnimation
    }

    [Header("CheckPoint")]
    public float Acceleration = 10.0f; //#TODO: should be on the player entity!!!

    //#TODO: move the actual respawning into the respawn or spawn component of the entity!!!
    [SerializeField] private float currentVeclocity;

    [SerializeField] private RespawnStateMachine currentState;
    [SerializeField] private CharacterMovement respawneeGroundMovementDynamicRef;
    [SerializeField] private CharacterSteering respawneeSteeringDynamicRef;
    [SerializeField] private PlayerController respawneePlayerControllerDynamicRef;

    [SerializeField] protected VfxPlayer vfxPlayerStaticRef; // maybe move to the SplinePointObject too
    //////////////////////// 

    // Start is called before the first frame update
    void Awake()
    {
        base.BaseAwakeSplinePointObject();
        vfxPlayerStaticRef = gameObject.GetComponent<VfxPlayer>();
        currentState = RespawnStateMachine.Inactive;
        splinePointObjectType = SplinePointObjectType.CheckPoint;
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
                    if(respawneeGroundMovementDynamicRef.IsAnimationFinished(respawneeGroundMovementDynamicRef.RespawnAnimation))
                    {
                        currentState = RespawnStateMachine.Moving;
                    }
                    break;
                }
            case RespawnStateMachine.Moving:
                {
                    if (respawneeSteeringDynamicRef.HasArrived())
                    {
                        respawneeGroundMovementDynamicRef.StopAndPlayAnimation(respawneeGroundMovementDynamicRef.SpawnAnimation);
                        currentState = RespawnStateMachine.PlayingSpawnAnimation;
                    }
                    break;
                }
            case RespawnStateMachine.PlayingSpawnAnimation:
                {   
                    if(respawneeGroundMovementDynamicRef.IsAnimationFinished(respawneeGroundMovementDynamicRef.SpawnAnimation))
                    {
                        respawneePlayerControllerDynamicRef.SetBlockInput(false);

                        currentVeclocity = 0.0f;
                        respawneePlayerControllerDynamicRef = null;
                        respawneeGroundMovementDynamicRef = null;
                        respawneeSteeringDynamicRef = null;
                        currentState = RespawnStateMachine.Inactive;
                    }
                    break;
                }
        }
    }

    public void Respawn(PlayerController playerController, CharacterMovement groundMovement, CharacterSteering characterSteering)
    {
        currentState = RespawnStateMachine.PlayingRespawnAnimation;
        respawneeGroundMovementDynamicRef = groundMovement;
        respawneePlayerControllerDynamicRef = playerController;
        respawneeSteeringDynamicRef = characterSteering;

        respawneeSteeringDynamicRef.StartSteeringOnPath(pointIndex, ContainingGroundLayer.MovementSpline, 5.0f);

        respawneeGroundMovementDynamicRef.StopAndPlayAnimation(respawneeGroundMovementDynamicRef.RespawnAnimation);
        respawneePlayerControllerDynamicRef.SetBlockInput(true);
    }

    // needs to be unified with interactable
    public void OnBecomeAvailable()
    {
        isHidden = true;
        vfxPlayerStaticRef.PlayVfxEnter(ContainingGroundLayer.SpriteLayerOrder, DetectionRadius * 2.0f);
    }
    public void OnBecomeUnavailable()
    {
        isHidden = false;
        vfxPlayerStaticRef.PlayVfxExit();
    }

}
