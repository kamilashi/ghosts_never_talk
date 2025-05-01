using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GNT;
using static UnityEditor.Progress;
using System;

namespace Pathfinding
{
    [Serializable]
    public struct AstarNode
    {
        public ControlPoint point;
        public float cost;
    }

    [Serializable]
    public class PriorityQueue
    {
        public List<AstarNode> binaryHeap;
        public PriorityQueue() 
        { 
            binaryHeap = new List<AstarNode>(); 
        }

        public void Enqueue(AstarNode newNode)
        {
            binaryHeap.Add(newNode);

            int newNodeIndex = binaryHeap.Count - 1;

            while (hasParent(newNodeIndex) && getParent(newNodeIndex).cost > newNode.cost) 
            {
                swapWithParent(newNodeIndex);
                newNodeIndex = getParentIndex(newNodeIndex);
            }
        }

        public AstarNode? Dequeue()
        {
            if (binaryHeap.Count == 0) 
            {
                return null;
            }

            AstarNode topNode = binaryHeap[0];

            binaryHeap.RemoveAt(0);

            return topNode;
        }

        private bool hasParent(int childIndex)
        {
            return childIndex != 0;
        } 
        private bool hasLeftChild(int parentIndex)
        {
            return getLeftChildIndex(parentIndex) < binaryHeap.Count;
        }
        private bool hasRightChild(int parentIndex)
        {
            return getRightChildIndex(parentIndex) < binaryHeap.Count;
        }

        // for index = 0 returns self
        private AstarNode getParent(int childIndex)
        {
            int parentIndex = childIndex == 0 ? 0 : getParentIndex(childIndex);
            return binaryHeap[parentIndex];
        }
        
        // for node at index = leaf returns self
        private AstarNode? getLeftChild(int parentIndex)
        {
            int childIndex = getLeftChildIndex(parentIndex);

            if (childIndex < binaryHeap.Count)
            {
                return binaryHeap[childIndex];
            }

            return null;
        }
        
        private AstarNode? getRightChild(int parentIndex)
        {
            int childIndex = getRightChildIndex(parentIndex);

            if (childIndex < binaryHeap.Count)
            {
                return binaryHeap[childIndex];
            }

            return null;
        }

        private void swapWithParent(int childIndex)
        {
            int parentIndex = getParentIndex(childIndex);
            AstarNode parentNode = binaryHeap[parentIndex];
            binaryHeap[parentIndex] = binaryHeap[childIndex];
            binaryHeap[childIndex] = parentNode;
        }

        private int getParentIndex(int childIndex)
        {
            return Mathf.FloorToInt((childIndex - 1) / 2);
        }
        private int getLeftChildIndex(int parentIndex)
        {
            return parentIndex * 2 + 1;
        }
        private int getRightChildIndex(int parentIndex)
        {
            return parentIndex * 2 + 2;
        }

        public string PrintToString()
        {
            int maxWidth = Mathf.CeilToInt (Screen.width / 3.0f);
            string output = "";
            int row = 0;
            int index = 0;

            do
            {
                int digitsInRow = 1 << row;

                string indent = new string(' ', maxWidth - digitsInRow * 4);
                output += indent;

                for (int i = 0; i < digitsInRow; i++)
                {
                    output += binaryHeap[index].cost + "  ";
                    index++;

                    if (index == binaryHeap.Count)
                    {
                        break;
                    }
                }

                output += "\n";
                row ++;
            }
            while (index < binaryHeap.Count);

            return output;
        }
      }

    public class PathFinder
    {
        Pathfinding.PriorityQueue queueTest = new Pathfinding.PriorityQueue();
    }
    }