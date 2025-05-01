using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

namespace GNT
{
    public class CharacterSteering : MonoBehaviour
    {
        Pathfinding.PriorityQueue queueTest = new Pathfinding.PriorityQueue();

        public List<AStarNode> binaryHeap;
        public float newValue;


        void Start()
        {

        }

        void Update()
        {

        }

        [ContextMenu("Push")]
        public void Push()
        {
            AStarNode node = new AStarNode(null, newValue, null);

            queueTest.Enqueue(node);

            binaryHeap = queueTest.binaryHeap;

            Debug.Log("Pushed " + newValue + ": \n" + queueTest.PrintToString());
        }
        
        [ContextMenu("Pop")]
        public void Pop()
        {
            AStarNode? node = queueTest.Dequeue();

            binaryHeap = queueTest.binaryHeap;

            Debug.Log("Popped " + node?.cost + ": \n" + queueTest.PrintToString());
        }
        
        [ContextMenu("Flush")]
        public void Flush()
        {
            queueTest = new PriorityQueue();

            binaryHeap = queueTest.binaryHeap;
        }
    }
}