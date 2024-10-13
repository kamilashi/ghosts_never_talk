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

        private int inputDirection;   // input speed received from controller
        private int inputSpeed;   // input speed received from controller

        private float currentHorizontalVelocity; // working speed that is used for smooth movement, range from - MaxSpeed to  MaxSpeed

        Collider2D collider2D;
        Animator animator;
        LayerMask groundCollisionMask; // move to global data

        void Awake()
        {
// #ToDo: refactor this
#if UNITY_EDITOR
            Assert.AreNotEqual(0,  SpeedValues[(int)GNT.MoveSpeed.Count-1]);
            Assert.AreNotEqual(0,  SpeedValues[(int)GNT.MoveSpeed.Count-2]);
            Assert.AreNotEqual(0, Acceleration);
#endif

            groundCollisionMask = LayerMask.GetMask("Ground");
            collider2D = gameObject.GetComponentInChildren<Collider2D>();
            animator = gameObject.GetComponentInChildren<Animator>();
        }

        void Start()
        {
            currentHorizontalVelocity = SpeedValues[(int)GNT.MoveSpeed.Stand];

            // play spawn animation and hop from the sky?
            snapToGround();
        }

        void Update()
        {
           currentHorizontalVelocity = SmoothingFuncitons.ApproachReferenceLinear(currentHorizontalVelocity, inputDirection*inputSpeed, Acceleration * Time.deltaTime);

            if(currentHorizontalVelocity != 0.0f)
            {
                moveAlongGroundCollisionNormal(currentHorizontalVelocity * Time.deltaTime);
            }

            float speedBlendAnimationInput = getNormalizedSpeedBlend(SpeedValues[(int)MoveSpeed.Walk]);
            int directionAninmationInput = getDirectionInt();
            animator.SetFloat("idleToWalkSpeedBlend", speedBlendAnimationInput);
            animator.SetInteger("directionInt", directionAninmationInput);
        }

        public void SetMovementInput(MoveDirection direction, MoveSpeed speed)
        {
            inputDirection = (int)direction;
            inputSpeed = SpeedValues[(int)speed];
        }

        // hacky
        private void snapToGround()
        {
            float rayLength = 10.0f;
            Vector3 rayStart = transform.position;
            RaycastHit2D downHit = Physics2D.Raycast(rayStart, Vector2.down, rayLength, groundCollisionMask);

            Vector3 down = Vector3.zero;
            down.y -= downHit.distance - (collider2D.bounds.center.y - collider2D.bounds.min.y); // snap to the ground.
            transform.Translate(down, Space.World);
        }

        private void moveHorizontally(float horizontalVelocitzPerTimeStep)
        {
            Vector3 horizontalVelocity = new Vector3(horizontalVelocitzPerTimeStep, 0.0f, 0.0f);
            transform.Translate(horizontalVelocity, Space.World);
        }

        private float getNormalizedSpeedBlend(float maxSpeed)
        {
            return Mathf.Abs(currentHorizontalVelocity / maxSpeed);
        }
        private int getDirectionInt()
        {
            return inputDirection;
        }

        private void moveAlongGroundCollisionNormal(float horizontalVelocitzPerTimeStep)
        {
            float rayLength = 10.0f;
            Vector3 rayStart = transform.position;
            RaycastHit2D downHit =  Physics2D.Raycast(rayStart, Vector2.down, rayLength, groundCollisionMask);

            Vector3 horizontalDirection = new Vector3(Mathf.Sign(horizontalVelocitzPerTimeStep), 0.0f, 0.0f);
            Vector3 groundNormal = downHit.normal;
            Vector3 alongNormal = Vector3.Cross(groundNormal, new Vector3(0.0f,0.0f,1.0f));
            alongNormal *= horizontalVelocitzPerTimeStep;

            alongNormal.y -= downHit.distance - (collider2D.bounds.center.y - collider2D.bounds.min.y); // snap to the ground.

#if UNITY_EDITOR
            // Vector3 endPosition = transform.position;
            //endPosition += alongNormal;
            //Debug.DrawLine(transform.position, endPosition, Color.magenta, Time.deltaTime, false);
#endif
            transform.Translate(alongNormal, Space.World);
        }
    }
}