using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Library;

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

    public class GroundMovement : MonoBehaviour
    {
        [SerializeField] // only settable in the inspector
        private int[] SpeedValues = new int[(int) GNT.MoveSpeed.Count];
        [SerializeField]
        private float Acceleration = 0.0f; // only settable in the inspector

        public LayerMask GroundCollisionMask;

        public AnimationClip teleportAnimation;

        private int inputDirection = 1;   // input speed received from controller
        private int inputSpeed = 0;   // input speed received from controller
        private bool isTurning = false;
        private bool freezeMovement = false;

        private float currentHorizontalVelocity; // working speed that is used for smooth movement, range from - MaxSpeed to  MaxSpeed

        Collider2D collider2D;
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
            collider2D = gameObject.GetComponentInChildren<Collider2D>();
            animator = gameObject.GetComponentInChildren<Animator>();
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        }

        void Start()
        {
            currentHorizontalVelocity = SpeedValues[(int)GNT.MoveSpeed.Stand];

            // play spawn animation and hop from the sky?
            SnapToGround();
        }

        void Update()
        {
            if(!freezeMovement)
            {
                float newHorizontalVelocity = SmoothingFuncitons.ApproachReferenceLinear(currentHorizontalVelocity, inputDirection * inputSpeed, Acceleration * Time.deltaTime);

                currentHorizontalVelocity = newHorizontalVelocity;

                if (currentHorizontalVelocity != 0.0f)
                {
                    moveAlongGroundCollisionTangent(currentHorizontalVelocity * Time.deltaTime);
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

        // hacky, should go once we have path movement
        public void SnapToGround()
        {
            float rayLength = 10.0f;

            Vector3 down = Vector3.zero;
            down.y = - GetDistanceToGroundCollider(transform.position, rayLength, collider2D, GroundCollisionMask);
            transform.Translate(down, Space.World);
        }

        public void StopAndPlayAnimation(AnimationClip animation)
        {
            ResetMovement();
            playAnimation(animation);
        }

/*
        public void TeleportTranslateToLayer( Vector3 translateDelta, int SpriteLayerOrder)
        {
            spriteRenderer.sortingOrder = SpriteLayerOrder;
            transform.Translate(translateDelta, Space.World);
        }*/

        // hacky, should go once we have path movement
        public static float GetDistanceToGroundCollider( Vector3 startPosition, float rayLength, Collider2D collider2D,  LayerMask groundCollisionMask)
        {
            RaycastHit2D downHit = Physics2D.Raycast(startPosition, Vector2.down, rayLength, groundCollisionMask);

            return (downHit.distance - (collider2D.bounds.center.y - collider2D.bounds.min.y)); // snap to the ground.
        }

        private float getNormalizedSpeedBlend(float maxSpeed)
        {
            return Mathf.Abs(currentHorizontalVelocity / maxSpeed);
        }
        private int getDirectionInt()
        {
            return inputDirection;
        }

        private void moveAlongGroundCollisionTangent(float horizontalVelocitzPerTimeStep)
        {
            float rayLength = 10.0f;
            float stepDistance = 2.0f;
            Vector3 rayStart = transform.position;
            RaycastHit2D downHit =  Physics2D.Raycast(rayStart, Vector2.down, rayLength, GroundCollisionMask);

            Vector3 horizontalDirection = new Vector3(Mathf.Sign(horizontalVelocitzPerTimeStep), 0.0f, 0.0f);
            Vector3 groundNormal = downHit.normal;
            Vector3 alongNormal = Vector3.Cross(groundNormal, new Vector3(0.0f,0.0f,1.0f));
            alongNormal *= horizontalVelocitzPerTimeStep;

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
        
        public Collider2D GetCollider()
        {
            return collider2D;
        }
    }
}