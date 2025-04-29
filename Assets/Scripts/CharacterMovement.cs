using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Library;
using System;

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
    struct SplineMovementData
    {
        public float positionOnSpline;
        //#TODOD: here should be the current spline dynamic ref!!!
        public SplinePointObject availableSplinePointObject;
    }

    [Serializable]
    struct GroundLayerData
    {
       // public int indexInScene;
        public GroundLayer currentGorundLayer;
    }

    public class CharacterMovement : MonoBehaviour
    {
        public int[] SpeedValues = new int[(int) GNT.MoveSpeed.Count];
        public float Acceleration = 0.0f; // only settable in the inspector

        //#TODO this should move to some animation mapper
        public AnimationClip TeleportAnimation;
        public AnimationClip RespawnAnimation; 
        public AnimationClip SpawnAnimation; 

        [SerializeField] float offsetFromGroundToPivot;
        [SerializeField] SplineMovementData splineMovementData;
        [SerializeField] GroundLayerData groundLayerData; // for now set inspector - later should be loaded + managed during the switch

        int inputDirection = 1;   // input (horizontal) direction received from controller
        int inputSpeed = 0;   // input speed received from controller
        bool isTurning = false;
        bool freezeMovement = false;

        float currentHorizontalVelocity; // working speed that is used for smooth movement, range from - MaxSpeed to  MaxSpeed

        SpriteRenderer spriteRendererStaticRef;
        Animator animatorStaticRef;
        AnimationPlayer animationPlayerStaticRef;

        void Awake()
        {
// #ToDo: refactor this
#if UNITY_EDITOR
            Assert.AreNotEqual(0,  SpeedValues[(int)GNT.MoveSpeed.Count-1]);
            Assert.AreNotEqual(0,  SpeedValues[(int)GNT.MoveSpeed.Count-2]);
            Assert.AreNotEqual(0, Acceleration);
#endif
            animatorStaticRef = gameObject.GetComponent<Animator>();
            spriteRendererStaticRef = gameObject.GetComponent<SpriteRenderer>();
            animationPlayerStaticRef = gameObject.GetComponent<AnimationPlayer>();

            splineMovementData.positionOnSpline = 0.0f; //#TODO to be loaded from save data probably, or from the spawn component
        }

        void Start()
        {
            currentHorizontalVelocity = SpeedValues[(int)GNT.MoveSpeed.Stand];


            MoveAlongSpline(0.0f);
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
        
/*
        public void StopAndPlayAnimation(string animationStateName)
        {
            ResetMovement();
            playAnimation(animationStateName);
        }*/

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
            CatmullRomSpline currentSpline = groundLayerData.currentGorundLayer.MovementSpline;
            Vector3 newPosition = currentSpline.GetPositionOnSpline(ref splineMovementData.positionOnSpline, ref splineMovementData.availableSplinePointObject, horizontalVelocityPerTimeStep);
            newPosition.y += offsetFromGroundToPivot;
            Vector3 toNewPosition = newPosition - transform.position;
            transform.Translate(toNewPosition, Space.World);
        }

        public void TeleportToSplinePoint(int pointIndex)
        {
            CatmullRomSpline currentSpline = groundLayerData.currentGorundLayer.MovementSpline;

            splineMovementData.positionOnSpline = currentSpline.GetLocalPositionOnSpline(pointIndex);
            MoveAlongSpline(0.0f);
        }
        
        public void TeleportToSplinePoint(int pointIndex, GroundLayer targteLayer)
        {
            CatmullRomSpline currentSpline = targteLayer.MovementSpline;

            if (targteLayer != groundLayerData.currentGorundLayer)
            {
                SwitchToLayer(targteLayer, currentSpline.GetLocalPositionOnSpline(pointIndex));
                spriteRendererStaticRef.sortingOrder = targteLayer.SpriteLayerOrder;
            }
            else
            {

                splineMovementData.positionOnSpline = currentSpline.GetLocalPositionOnSpline(pointIndex);
                MoveAlongSpline(0.0f);
            }
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
        }
        public SplinePointObject GetAvailableSplinePointObject()
        {
            return splineMovementData.availableSplinePointObject;
        }
        public GroundLayer GetCurrentGroundLayer()
        {
            return groundLayerData.currentGorundLayer;
        }

        public bool IsAtSplinePoint(int pointIndex, float error = 0.01f)
        {
            return Mathf.Abs(GetAbsoluteDistanceToSplinePoint(pointIndex)) <= error;
        }

        public float GetAbsoluteDistanceToSplinePoint(int pointIndex)
        {
            //#TODO this might have to change if we have multiple splines per ground layer. Also active walking layer SHOULD live on the entity 
            CatmullRomSpline currentSpline = groundLayerData.currentGorundLayer.MovementSpline;
            return currentSpline.GetLocalPositionOnSpline(pointIndex) - splineMovementData.positionOnSpline;
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
            groundLayerData.currentGorundLayer = targetGroundLayer;

            if(positionOnLayer >= 0.0)
            {
                splineMovementData.positionOnSpline = positionOnLayer;
                MoveAlongSpline(0.0f);
            }

            Debug.Log("switched to layerIdx " + targetGroundLayer.GroundLayerIndex);
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
    }
}