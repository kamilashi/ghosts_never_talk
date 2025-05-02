using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

namespace GNT
{
    public class CharacterSteering : MonoBehaviour
    {
        [Header("Sorting test")]
        Pathfinding.PriorityQueue queueTest = new Pathfinding.PriorityQueue();
        public List<AStarNode> binaryHeapTest;
        public float newValueTest;

        [Header("Pathfinder test")]
        [SerializeField] Pathfinding.CatmullRomSpline startSpline;
        [SerializeField] int sourcePointIndexTest;
        [SerializeField] Pathfinding.CatmullRomSpline targetSpline;
        [SerializeField] int targetPointIndexTest;

        [Header("Functional")]
        CharacterMovement characterMovementStaticRef;
        public List<Pathfinding.PathLink> path;

        //Pathfinding.Pathfinder pathfinder = new Pathfinding.Pathfinder();

        [SerializeField] private float currentVelocity;
        [SerializeField] private float maxSpeed;
        private ControlPoint target;

        void Awake()
        {
            characterMovementStaticRef = this.gameObject.GetComponent<CharacterMovement>();
        }

        void Update()
        {

        }

        public void MoveOnPathTo(ControlPoint targetControlPoint, float maxSpeed)
        {
            ControlPoint startPoint = characterMovementStaticRef.GetLastVisitedSplinePoint();
            path = Pathfinding.Pathfinder.GetAStarPath(characterMovementStaticRef.GetCurrentGroundLayer().MovementSpline, startPoint, target);

            this.target = targetControlPoint;
            this.maxSpeed = maxSpeed;
        }

        [ContextMenu("FindPath")]
        private void FindPathTest()
        {
            bool ignoreVisited = false;
            path.Clear();
            path = Pathfinding.Pathfinder.GetAStarPath(startSpline, startSpline.GetControlPoint(sourcePointIndexTest), targetSpline.GetControlPoint(targetPointIndexTest), ignoreVisited);
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