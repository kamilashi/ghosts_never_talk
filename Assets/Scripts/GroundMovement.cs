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
        Left = -1, // hard coded - should be relative to camera view?
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
    }

    public class GroundMovement : MonoBehaviour
    {
        public int[] SpeedValues = new int[(int) GNT.MoveSpeed.Count];
        public float Acceleration = 0.0f; // only settable in the inspector
        

        public LayerMask GroundCollisionMask;

        public AnimationClip TeleportAnimation; // this should move to some animation mapper

        public float offsetFromGroundToPivot;
        [SerializeField] SplineMovementData splineMovementData;

        int inputDirection = 1;   // input (horizontal) direction received from controller
        int inputSpeed = 0;   // input speed received from controller
        bool isTurning = false;
        bool freezeMovement = false;

        float currentHorizontalVelocity; // working speed that is used for smooth movement, range from - MaxSpeed to  MaxSpeed

        //Collider2D collider2D;
        SpriteRenderer spriteRenderer;
        Animator animator;

        void Awake()
        {
// #ToDo: refactor this
#if UNITY_EDITOR
            Assert.AreNotEqual(0,  SpeedValues[(int)GNT.MoveSpeed.Count-1]);
            Assert.AreNotEqual(0,  SpeedValues[(int)GNT.MoveSpeed.Count-2]);
            Assert.AreNotEqual(0, Acceleration);
#endif
           // collider2D = gameObject.GetComponent<Collider2D>();
            animator = gameObject.GetComponent<Animator>();
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

            splineMovementData.positionOnSpline = 0.5f; // to be loaded from save data probably
        }

        void Start()
        {
            currentHorizontalVelocity = SpeedValues[(int)GNT.MoveSpeed.Stand];

            // play spawn animation and hop from the sky?
            //SnapToGround();
            moveAlongSpline(0.0f);
        }

        void Update()
        {
            if(!freezeMovement)
            {
                float newHorizontalVelocity = SmoothingFuncitons.ApproachReferenceLinear(currentHorizontalVelocity, inputDirection * inputSpeed, Acceleration * Time.deltaTime);

                currentHorizontalVelocity = newHorizontalVelocity;

                if (currentHorizontalVelocity != 0.0f)
                {
                    moveAlongSpline(currentHorizontalVelocity * Time.deltaTime);
                }

                float speedBlendAnimationInput = getNormalizedSpeedBlend(SpeedValues[(int)MoveSpeed.Walk]);
                int directionAninmationInput = getDirectionInt();
                animator.SetFloat("idleToWalkSpeedBlend", speedBlendAnimationInput);
                animator.SetInteger("directionInt", directionAninmationInput);
            }
        }

        public void SetMovementInput(MoveDirection direction, MoveSpeed speed)
        {
            if((int)direction * inputDirection < 0.0f && !isTurning)
            {
                animator.SetBool("triggerTurning",true );
                isTurning = true;
            }

            inputDirection = (int)direction;
            inputSpeed = SpeedValues[(int)speed];
        }

        public void OnTurnFinishedAnimationEvent()
        {
            animator.SetBool("triggerTurning", false);

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

        private void moveAlongSpline(float horizontalVelocityPerTimeStep)
        {
            //#TODO this might have to change if we have multiple splines per ground layer
            CatmullRomSpline currentSpline = GlobalData.Instance.ActiveSceneDynamicRef.ActiveGroundLayer.MovementSpline;
            Vector3 newPosition = currentSpline.GetPositionOnSpline(ref splineMovementData.positionOnSpline, horizontalVelocityPerTimeStep);
            newPosition.y += offsetFromGroundToPivot;
            Vector3 toNewPosition = newPosition - transform.position;
            transform.Translate(toNewPosition, Space.World);
        }

        public void TeleportToSplinePoint(int pointIndex)
        {
            //#TODO this might have to change if we have multiple splines per ground layer
            CatmullRomSpline currentSpline = GlobalData.Instance.ActiveSceneDynamicRef.ActiveGroundLayer.MovementSpline;
            splineMovementData.positionOnSpline = currentSpline.GetLocalPositionOnSpline(pointIndex);
            moveAlongSpline(0.0f);
        }

        private void playAnimation(AnimationClip animationClip)
        {
            string teleportAnimationClipName = animationClip.name;
            animator.CrossFade(teleportAnimationClipName, 0.5f);
        }

        public void SetFreezeMovement(bool isEnabled)
        {
            freezeMovement = isEnabled;
        }
        public void ResetMovement()
        {
            SetMovementInput((GNT.MoveDirection) inputDirection, MoveSpeed.Stand);
        }

        
/*
        public static float GetDistanceToGroundCollider(Vector3 startPosition, float rayLength, Collider2D collider2D, LayerMask groundCollisionMask)
        {
            RaycastHit2D downHit = Physics2D.Raycast(startPosition, Vector2.down, rayLength, groundCollisionMask);

            return (downHit.distance - (collider2D.bounds.center.y - collider2D.bounds.min.y)); // snap to the ground.
        }
        public void SnapToGround()
        {
            float rayLength = 10.0f;

            Vector3 down = Vector3.zero;
            down.y = -GetDistanceToGroundCollider(transform.position, rayLength, collider2D, GroundCollisionMask);
            transform.Translate(down, Space.World);
        }
        private void moveAlongGroundCollisionTangent(float horizontalVelocityPerTimeStep)
        {
            float rayLength = 10.0f;
            float stepDistance = 2.0f;
            Vector3 rayStart = transform.position;
            RaycastHit2D downHit = Physics2D.Raycast(rayStart, Vector2.down, rayLength, GroundCollisionMask);

            Vector3 horizontalDirection = new Vector3(Mathf.Sign(horizontalVelocityPerTimeStep), 0.0f, 0.0f);
            Vector3 groundNormal = downHit.normal;
            Vector3 alongNormal = Vector3.Cross(groundNormal, new Vector3(0.0f, 0.0f, 1.0f));
            alongNormal *= horizontalVelocityPerTimeStep;

            alongNormal.y += stepDistance;

#if UNITY_EDITOR
            // Vector3 endPosition = transform.position;
            //endPosition += alongNormal;
            //Debug.DrawLine(transform.position, endPosition, Color.magenta, Time.deltaTime, false);
#endif
            Vector3 groundProbePosition = transform.position;
            groundProbePosition += alongNormal;

            alongNormal.y -= GetDistanceToGroundCollider(groundProbePosition, rayLength, collider2D, GroundCollisionMask);
            transform.Translate(alongNormal, Space.World);
        }

        public Collider2D GetCollider()
        {
            return collider2D;
        }*/
    }
}