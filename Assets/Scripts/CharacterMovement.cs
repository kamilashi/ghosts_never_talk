using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Library;
using System;
using Pathfinding;

namespace GNT
{
    public enum MoveDirection:int
    {
        Left = -1, // relative to splines local space
        Right = 1
    };

    public enum MoveSpeed:int
    {
        Stand,
        Walk,
        Run,
        Count
    };

    [Serializable]
    public struct SplineMovementData
    {
        public float positionOnSpline;
        //#TODOD: here should be the current spline dynamic ref!!!
        public SplinePointObject availableSplinePointObject;
        public ControlPoint lastVisitedControlPoint;
    }

    [Serializable]
    public struct GroundLayerData
    {
        public GroundLayer currentGorundLayer;
    }

    public class CharacterMovement : MonoBehaviour
    {
        [Header("Setup in Prefab")]

        public int[] SpeedValues = new int[(int) GNT.MoveSpeed.Count];
        public float Acceleration = 0.0f; // only settable in the inspector

        //#TODO this should move to some animation mapper
        public AnimationClip TeleportAnimation;
        public AnimationClip RespawnAnimation; 
        public AnimationClip SpawnAnimation;

        public float offsetFromGroundToPivot;

        public List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();

        [Header("Auto Setup")]

        Animator animatorStaticRef;
        AnimationPlayer animationPlayerStaticRef;

        [Header("Debug View")]

        public SplineMovementData splineMovementData;
        public GroundLayerData groundLayerData; // for now set inspector - later should be loaded + managed during the switch

        int inputDirection = 1;   // input (horizontal) direction received from controller
        int inputSpeed = 0;   // input speed received from controller
        bool isTurning = false;
        bool freezeMovement = false;

        float currentHorizontalVelocity; // working speed that is used for smooth movement, range from - MaxSpeed to  MaxSpeed

        public Action<int, int> LayerSwitchEvent;

        void Awake()
        {
// #ToDo: refactor this
#if UNITY_EDITOR
            Assert.AreNotEqual(0,  SpeedValues[(int)GNT.MoveSpeed.Count-1]);
            Assert.AreNotEqual(0,  SpeedValues[(int)GNT.MoveSpeed.Count-2]);
            Assert.AreNotEqual(0, Acceleration);
#endif
            animatorStaticRef = gameObject.GetComponent<Animator>();
            animationPlayerStaticRef = gameObject.GetComponent<AnimationPlayer>();
        }

        void Start()
        {
            currentHorizontalVelocity = SpeedValues[(int)GNT.MoveSpeed.Stand];
        }

        void Update()
        {
            if(!freezeMovement)
            {
                float newHorizontalVelocity = SmoothingFuncitons.ApproachReferenceLinear(currentHorizontalVelocity, inputDirection * inputSpeed, Acceleration * Time.deltaTime);

                currentHorizontalVelocity = newHorizontalVelocity;

                if (currentHorizontalVelocity != 0.0f)
                {
                    MoveAlongSpline(currentHorizontalVelocity * Time.deltaTime);
                }

                float speedBlendAnimationInput = getNormalizedSpeedBlend(SpeedValues[(int)MoveSpeed.Walk]);
                int directionAninmationInput = getDirectionInt();
                animatorStaticRef.SetFloat("idleToWalkSpeedBlend", speedBlendAnimationInput);
                animatorStaticRef.SetInteger("directionInt", directionAninmationInput);
            }
        }

        public void SetMovementInput(MoveDirection direction, MoveSpeed speed)
        {
            if((int)direction * inputDirection < 0.0f && !isTurning)
            {
                animatorStaticRef.SetBool("triggerTurning",true );
                isTurning = true;
            }

            inputDirection = (int)direction;
            inputSpeed = SpeedValues[(int)speed];
        }

        public void AddSplineLocalOffset(float delta)
        {
            splineMovementData.positionOnSpline += delta;
        }

        public void OnTurnFinishedAnimationEvent()
        {
            animatorStaticRef.SetBool("triggerTurning", false);

            Vector3 Lscale = transform.localScale;
            Lscale.x *= -1.0f;
            transform.localScale = Lscale;

            isTurning = false;
        }

        public bool IsTurning()
        {
            return isTurning;
        }

        public void StopAndPlayAnimation(AnimationClip animation)
        {
            ResetMovement();
            playAnimation(animation);
        }
        

        private float getNormalizedSpeedBlend(float maxSpeed)
        {
            return Mathf.Abs(currentHorizontalVelocity / maxSpeed);
        }
        private int getDirectionInt()
        {
            return inputDirection;
        }

        public void MoveAlongSpline(float horizontalVelocityPerTimeStep)
        {
            //#TODO this might have to change if we have multiple splines per ground layer
            Pathfinding.CatmullRomSpline currentSpline = groundLayerData.currentGorundLayer.MovementSpline;
            Vector3 newPosition = currentSpline.GetPositionOnSpline(ref splineMovementData, horizontalVelocityPerTimeStep);
            newPosition.y += offsetFromGroundToPivot;
            Vector3 toNewPosition = newPosition - transform.position;
            transform.Translate(toNewPosition, Space.World);
        }

        public void TeleportToSplinePoint(int pointIndex)
        {
            Pathfinding.CatmullRomSpline currentSpline = groundLayerData.currentGorundLayer.MovementSpline;

            SetLocalPositionOnSpline(currentSpline.GetLocalPositionOnSpline(pointIndex));
        }
        
        public void TeleportToSplinePoint(int pointIndex, GroundLayer targetLayer)
        {
            if (targetLayer != groundLayerData.currentGorundLayer)
            {
                SwitchToLayer(targetLayer);
            }

            TeleportToSplinePoint(pointIndex);
        }

        private void playAnimation(AnimationClip animationClip)
        {
            string animationClipName = animationClip.name;
            animatorStaticRef.CrossFade(animationClipName, 0.0f);
        }

        public void SetFreezeMovement(bool isEnabled)
        {
            freezeMovement = isEnabled;
        }
        public void ResetMovement()
        {
            SetMovementInput((GNT.MoveDirection) inputDirection, MoveSpeed.Stand);
            currentHorizontalVelocity = 0.0f;   
        }
        public SplinePointObject GetAvailableSplinePointObject()
        {
            return splineMovementData.availableSplinePointObject;
        }
        public Pathfinding.ControlPoint GetLastVisitedSplinePoint()
        {
            return splineMovementData.lastVisitedControlPoint;
        }
        public GroundLayer GetCurrentGroundLayer()
        {
            return groundLayerData.currentGorundLayer;
        }

        public bool IsAtSplinePoint(int pointIndex, Pathfinding.CatmullRomSpline spline = null, float error = 0.01f)
        {
            Pathfinding.CatmullRomSpline currentSpline = groundLayerData.currentGorundLayer.MovementSpline;
            bool isSplineCorrect = spline == null || currentSpline == spline;
            return isSplineCorrect && Mathf.Abs(GetSignedDistanceToPointOnCurrentSpline(pointIndex)) <= error;
        }

        public float GetSignedDistanceToPointOnCurrentSpline(int pointIndex)
        {
            //#TODO this might have to change if we have multiple splines per ground layer. Also active walking layer SHOULD live on the entity 

            return groundLayerData.currentGorundLayer.MovementSpline.GetLocalPositionOnSpline(pointIndex) - splineMovementData.positionOnSpline;
        }


        // #TODO: Move this to an animationPlayer component!!!!
        public bool IsAnimationFinished(AnimationClip animation)
        {
            AnimatorStateInfo stateInfo = animatorStaticRef.GetCurrentAnimatorStateInfo(0);
            int animationHash = Animator.StringToHash(animation.name);

            return animationPlayerStaticRef.HasAnimationFinished(animationHash);
        }
        public void SwitchToLayer(GroundLayer targetGroundLayer, float positionOnLayer = -1.0f)
        {
            int oldLayerId = groundLayerData.currentGorundLayer? groundLayerData.currentGorundLayer.GroundLayerIndex : targetGroundLayer.GroundLayerIndex;

            groundLayerData.currentGorundLayer = targetGroundLayer;
            SetSpriteOrder(targetGroundLayer.SpriteLayerOrder + 1); // on top of the ground layer

            if (positionOnLayer >= 0.0)
            {
                SetLocalPositionOnSpline(positionOnLayer);
            }
            else if(splineMovementData.positionOnSpline > groundLayerData.currentGorundLayer.MovementSpline.GetTotalLength())
            {
                SetLocalPositionOnSpline(groundLayerData.currentGorundLayer.MovementSpline.GetTotalLength());
            }

            LayerSwitchEvent?.Invoke(oldLayerId, targetGroundLayer.GroundLayerIndex);

           Debug.Log("switched to layerIdx " + targetGroundLayer.GroundLayerIndex);
        }
        public void SetLocalPositionOnSpline(float positionOnLayer)
        {
            splineMovementData.positionOnSpline = positionOnLayer;
            MoveAlongSpline(0.0f);
        }

        public bool SwitchIn()
        {
            GroundLayer targetGroundLayer = GameManager.Instance.ActiveSceneDynamicRef.GetFartherOrThisGroundLayer(groundLayerData.currentGorundLayer.GroundLayerIndex);

            if(targetGroundLayer != groundLayerData.currentGorundLayer)
            {
                SwitchToLayer(targetGroundLayer);
                return true;
            }

            return false;
        }

        public bool SwitchOut()
        {
            GroundLayer targetGroundLayer = GameManager.Instance.ActiveSceneDynamicRef.GetCloserOrThisGroundLayer(groundLayerData.currentGorundLayer.GroundLayerIndex);

            if (targetGroundLayer != groundLayerData.currentGorundLayer)
            {
                SwitchToLayer(targetGroundLayer);
                return true;
            }

            return false;
        }

        private void SetSpriteOrder(int layerOrder)
        {
            foreach(SpriteRenderer spriteRenderer in spriteRenderers)
            {
                spriteRenderer.sortingOrder = layerOrder;
            }
        }
    }
}