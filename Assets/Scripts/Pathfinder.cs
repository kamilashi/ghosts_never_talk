using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GNT;
using static UnityEditor.Progress;
using System;
using UnityEngine.U2D;
using TreeEditor;
using UnityEditor.Search;
using System.Linq;
using static UnityEngine.GraphicsBuffer;

namespace Pathfinding
{
    [Serializable]
    public class AStarNode
    {
        public ControlPoint point;
        //public int controlPointIndex;
        public float cost;
        public AStarNode prevNode;

        public AStarNode(ControlPoint p, /*int index, */float c, AStarNode pNode)
        {
            point = p; 
            //controlPointIndex = index;
            cost = c;
            prevNode = pNode;
        }
    }

    [Serializable]
    public class PriorityQueue
    {
        public List<AStarNode> binaryHeap;
        public PriorityQueue() 
        { 
            binaryHeap = new List<AStarNode>(); 
        }

        public void Enqueue(AStarNode newNode)
        {
            binaryHeap.Add(newNode);

            sortUp (binaryHeap.Count - 1);
        }

        public AStarNode? Dequeue()
        {
            if (binaryHeap.Count == 0) 
            {
                return null;
            }

            AStarNode topNode = binaryHeap[0];
            binaryHeap[0] = binaryHeap[binaryHeap.Count - 1];

            binaryHeap.RemoveAt(binaryHeap.Count - 1);
            sortDown(0);

            return topNode;
        }

        private void sortUp(int startIndex)
        {
            int index = startIndex;
            while (hasParent(index) && getParent(index).cost > binaryHeap[index].cost)
            {
                swapWithParent(index);
                index = getParentIndex(index);
            }
        }
        
        private void sortDown(int startIndex)
        {
            int index = startIndex;
            while (hasLeftChild(index) || hasRightChild(index))
            {
                int minCostChildIndex;

                if (!hasRightChild(index))
                {
                    minCostChildIndex = getLeftChildIndex(index);
                }
                else
                {
                    if (getLeftChild(index).cost < getRightChild(index).cost)
                    {
                        minCostChildIndex = getLeftChildIndex(index);
                    }
                    else
                    {
                        minCostChildIndex = getRightChildIndex(index);
                    }
                }

                if (binaryHeap[minCostChildIndex].cost > binaryHeap[index].cost)
                {
                    break;
                }

                swapWithParent(minCostChildIndex);
                index = minCostChildIndex;
            }
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
        private AStarNode getParent(int childIndex)
        {
            int parentIndex = childIndex <= 0 ? 0 : getParentIndex(childIndex);
            return binaryHeap[parentIndex];
        }
        
        // for node at index = leaf returns self
        private AStarNode? getLeftChild(int parentIndex)
        {
            int childIndex = getLeftChildIndex(parentIndex);

            if (childIndex < binaryHeap.Count)
            {
                return binaryHeap[childIndex];
            }

            return null;
        }
        
        private AStarNode? getRightChild(int parentIndex)
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
            AStarNode parentNode = binaryHeap[parentIndex];
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
            int maxWidth = Mathf.CeilToInt (640 / 3);
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

        public bool IsEmpty()
        { 
            return binaryHeap.Count == 0;
        }

        public bool Contains(ref AStarNode foundNode, ControlPoint referencePoint)
        {
            AStarNode node = binaryHeap.Find(e => e.point == referencePoint);

            if (node != null)
            {
                foundNode = node;
                return true;
            }
            
            return false;
        }

        public void Remove(AStarNode node)
        {
            if (binaryHeap.Contains(node))
            {
                binaryHeap.Remove(node);
            } 
        }
      }

    public struct MoveCommand
    {
        int targetPointIndex;
        bool isLinkedPoint;
    }

    public class Pathfinder
    {
       public List<MoveCommand> Path;

        public void SetPath(List<MoveCommand> path)
        {
            Path = path;
        }

        public static List<MoveCommand> GetAStarPath(Pathfinding.CatmullRomSpline startSpline, Pathfinding.ControlPoint source, Pathfinding.ControlPoint target)
        {
            Pathfinding.PriorityQueue priorityQueue = new Pathfinding.PriorityQueue();
            Pathfinding.CatmullRomSpline spline = startSpline;
            List<Pathfinding.ControlPoint> processed = new List<Pathfinding.ControlPoint>();
            List<MoveCommand> path = new List<MoveCommand>();

            AStarNode node = new AStarNode(source, 0.0f, null);
            priorityQueue.Enqueue(node);

            while (!priorityQueue.IsEmpty())
            {
                AStarNode topNode = priorityQueue.Dequeue();

                if (topNode.point == target) // target reached
                {
                    return path;
                }

                Action<Pathfinding.ControlPoint> processPoint = pointToProcess =>
                {
                    if (pointToProcess != null && pointToProcess.wasVisited && !processed.Contains(pointToProcess))
                    {
                        AStarNode newNode = toAStarNode(pointToProcess, target, topNode);
                        AStarNode queuedNode = newNode;

                        if (priorityQueue.Contains(ref queuedNode, newNode.point) && queuedNode.cost > newNode.cost) // if found a short cut
                        {
                            priorityQueue.Remove(queuedNode);
                        }

                        priorityQueue.Enqueue(newNode);
                    }
                };

                int pointIndex = startSpline.GetControlPointIndex(topNode.point);
                ControlPoint left = startSpline.GetLeftPoint(pointIndex);
                processPoint(left);

                ControlPoint right = startSpline.GetRightPoint(pointIndex);
                processPoint(right);

                ControlPoint vertical = startSpline.GetLinkedPoint(pointIndex);
                processPoint(vertical);

                processed.Add(topNode.point);
            }

            // target cannot be reached!

            return path;
        }

        private static AStarNode toAStarNode(Pathfinding.ControlPoint currentPoint, Pathfinding.ControlPoint target, AStarNode prevNode)
        {
            float nodeCost = Mathf.Abs(currentPoint.localPos - prevNode.point.localPos);
            float remainingCost = (target.GetPosition() - currentPoint.GetPosition()).sqrMagnitude;

            float cost = prevNode.cost + nodeCost + remainingCost;

            AStarNode node = new AStarNode(currentPoint, cost, prevNode);
            return node;
        }
    }
}