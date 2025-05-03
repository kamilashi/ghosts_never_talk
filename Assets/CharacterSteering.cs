using Library;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using static CheckPoint;

namespace GNT
{
    public class CharacterSteering : MonoBehaviour
    {
        public enum SteeringStateMachine
        {
            Inactive,
            MoveToTarget,
            Arrived
        }

        [Header("Sorting test")]
        Pathfinding.PriorityQueue queueTest = new Pathfinding.PriorityQueue();
        public List<AStarNode> binaryHeapTest;
        public float newValueTest;

        [Header("Pathfinder test")]
        public Pathfinding.CatmullRomSpline startSplineTest;
        public int sourcePointIndexTest;
        public Pathfinding.CatmullRomSpline targetSplineTest;
        public int targetPointIndexTest;

        [Header("Functional")]
        CharacterMovement characterMovementStaticRef;

        [SerializeField] private float currentVelocity;
        [SerializeField] private float maxSpeed;
        [SerializeField] private SteeringStateMachine currentState;

        private int targetPointIndex;
        private Pathfinding.ControlPoint targetPoint;
        private Pathfinding.CatmullRomSpline targetSpline;
        private bool autoDisableOnArrival;

        public List<Pathfinding.PathLink> path;
        private Pathfinding.PathLink nextTarget;

        void Awake()
        {
            characterMovementStaticRef = this.gameObject.GetComponent<CharacterMovement>();
            StopSteering();
        }

        void Update()
        {
            switch (currentState)
            {
                case SteeringStateMachine.Inactive:
                    break;
                case SteeringStateMachine.MoveToTarget:
                    ProcessSteering();
                    break;
                case SteeringStateMachine.Arrived:
                    if (autoDisableOnArrival)
                    {
                        StopSteering();
                    }
                    break;
                default:
                    break;
            }
        }

        public void StartSteeringOnPath(int targetControlPointIndex, Pathfinding.CatmullRomSpline targetSpline, float maxSpeed, bool autoDisableOnArrival = true)
        {
            targetPointIndex = targetControlPointIndex;
            Pathfinding.ControlPoint targetPoint = targetSpline.GetControlPoint(targetControlPointIndex);
            StartSteeringOnPath(targetPoint, maxSpeed, autoDisableOnArrival);
        }

        public void StartSteeringOnPath(Pathfinding.ControlPoint targetControlPoint, float maxSpeed, bool autoDisableOnArrival = true)
        {

            Pathfinding.ControlPoint startPoint = characterMovementStaticRef.GetLastVisitedSplinePoint();
            path = Pathfinding.Pathfinder.GetAStarPath(ref targetSpline, characterMovementStaticRef.GetCurrentGroundLayer().MovementSpline, startPoint, targetControlPoint);

            if (path.Count == 0) 
            {
                StopSteering();
                return;
            }

            this.targetPoint = targetControlPoint;
            this.maxSpeed = maxSpeed;
            this.autoDisableOnArrival = autoDisableOnArrival;
            this.currentState = SteeringStateMachine.MoveToTarget;

            if (targetPointIndex < 0)
            {
                targetPointIndex = targetSpline.GetControlPointIndex(targetControlPoint);
            }

            setNextTarget(startPoint);
        }

        public void StopSteering()
        {
            this.currentState = SteeringStateMachine.Inactive;
            path.Clear();
            targetPoint = null;  
            targetSpline = null;
            nextTarget = null;
            maxSpeed = 0.0f;
            currentVelocity = 0.0f;
            targetPointIndex = -1;  
        }

        public void ProcessSteering()
        {
            float error = path.Count == 0 ? 0.01f : 0.5f;

            if (characterMovementStaticRef.IsAtSplinePoint(nextTarget.index, nextTarget.spline, error))
            {
                if(path.Count == 0)
                {
                    currentState = SteeringStateMachine.Arrived;
                }
                else
                {
                    Pathfinding.PathLink reachedTarget = new Pathfinding.PathLink();
                    reachedTarget = nextTarget;

                    setNextTarget(reachedTarget.targetPoint);
                    Debug.Assert(reachedTarget != nextTarget);
                }
            }
            else
            {
                bool stopAtTarget = nextTarget.targetPoint == targetPoint;
                float direction = Mathf.Sign((int)nextTarget.direction);
                float absoluteDistance = stopAtTarget ? characterMovementStaticRef.GetSignedDistanceToPointOnCurrentSpline(nextTarget.index) : direction * float.MaxValue;

                currentVelocity = SmoothingFuncitons.ApproachReferenceLinear(currentVelocity, direction * maxSpeed, characterMovementStaticRef.Acceleration * Time.deltaTime);

                float translation = Helpers.MinValue (currentVelocity * Time.deltaTime, Mathf.Abs (absoluteDistance));


                characterMovementStaticRef.MoveAlongSpline(translation);
            }
        }

        private void setNextTarget(Pathfinding.ControlPoint currentPoint)
        {
            nextTarget = path[0];
            path.RemoveAt(0);

            if (nextTarget.direction == Pathfinding.LinkDirection.Vertical)
            {
                Debug.Assert(currentPoint.objectAtPoint != null);
                ((InteractableTeleporter)currentPoint.objectAtPoint).Teleport(characterMovementStaticRef);
            }
        }

        public bool HasArrived()
        {
            return currentState == SteeringStateMachine.Arrived;
        }

        [ContextMenu("FindPath")]
        private void FindPathTest()
        {
            bool ignoreVisited = false;
            path.Clear();
            path = Pathfinding.Pathfinder.GetAStarPath(ref targetSpline, startSplineTest, startSplineTest.GetControlPoint(sourcePointIndexTest), targetSplineTest.GetControlPoint(targetPointIndexTest), ignoreVisited);
        }

        [ContextMenu("Push")]
        public void Push()
        {
            AStarNode node = new AStarNode(null, newValueTest, null, LinkDirection.None, null, 0);

            queueTest.Enqueue(node);

            binaryHeapTest = queueTest.binaryHeap;

            Debug.Log("Pushed " + newValueTest + ": \n" + queueTest.PrintToString());
        }
        
        [ContextMenu("Pop")]
        public void Pop()
        {
            AStarNode? node = queueTest.Dequeue();

            binaryHeapTest = queueTest.binaryHeap;

            Debug.Log("Popped " + node?.cost + ": \n" + queueTest.PrintToString());
        }
        
        [ContextMenu("Flush")]
        public void Flush()
        {
            queueTest = new PriorityQueue();

            binaryHeapTest = queueTest.binaryHeap;
        }
    }
}