using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GNT
{
    public class InteractableTeleporter : Interactable
    {
        [Header("Teleporter")]
        public InteractableTeleporter TargetTeleporter;
        //public bool IsReceiverOnly;

        private void Awake()
        {
            BaseAwakeInteractable();
            splinePointObjectType = SplinePointObjectType.InteractableTeleporter;
        }

        private void Start()
        {
        }

        public override void OnBecomeAvailable()
        {
           // Debug.Log("Teleport available");
            base.OnBecomeAvailable();
        }
        public override void OnBecomeUnavailable()
        {
           // Debug.Log("Teleport unavailable");
            base.OnBecomeUnavailable();
        }
        
        public void Teleport(CharacterMovement teleporteeGroundMovement)
        {
            teleporteeGroundMovement.TeleportToSplinePoint(TargetTeleporter.pointIndex, TargetTeleporter.ContainingGroundLayer);
        }


/*
        public override void ExecuteSplineObject(PlayerController playerControllerRef = null, CharacterMovement groundMovementRef = null)
        {
            base.ExecuteSplineObject(playerControllerRef, groundMovementRef);
        }*/

        public bool isReceiverOnly()
        {
            return TargetTeleporter == null;
        }
    }
}