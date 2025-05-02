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
    public enum LinkDirection
    {
        Left = -1,
        Vertical = 0,
        Right = 1,
        None = 10
    }

    [Serializable]
    public class PathLink
    {
        public ControlPoint targetPoint;
        public int index;
        public LinkDirection direction;
    }

    [Serializable]
    public class AStarNode
    {
        public ControlPoint point;
        public LinkDirection direction;
        public float cost;
        public AStarNode prevNode;
        public CatmullRomSpline spline;
        public int index;

        public AStarNode(ControlPoint p, float c, AStarNode pNode, LinkDirection prevToThisDir, CatmullRomSpline parentSpline, int pointIndex)
        {
            point = p; 
            //controlPointIndex = index;
            cost = c;
            prevNode = pNode;
            direction = prevToThisDir;
            spline = parentSpline;
            index = pointIndex;
        }

        public PathLink ToPathLink()
        {
            PathLink link = new PathLink();
            link.direction = direction;
            link.targetPoint = point;
            link.index = index;
            return link;
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

        public void Replace(AStarNode oldNode, AStarNode newNode)
        {
            if (binaryHeap.Contains(oldNode))
            {
                int index = binaryHeap.IndexOf(oldNode);
                binaryHeap[index] = newNode;
                sortUp(index);
            } 
        }
      }

    public class Pathfinder
    {
        /*
        public List<MoveCommand> Path;
        public void SetPath(List<MoveCommand> path)
        {
            Path = path;
        }*/

        public static List<PathLink> GetAStarPath(Pathfinding.CatmullRomSpline startSpline, Pathfinding.ControlPoint source, Pathfinding.ControlPoint target, bool ignoreVisitedCondition = false)
        {
            Pathfinding.PriorityQueue priorityQueue = new Pathfinding.PriorityQueue();
            Dictionary<ControlPoint, AStarNode> processed = new Dictionary<ControlPoint, AStarNode>();
            List<PathLink> path = new List<PathLink>();

            AStarNode node = new AStarNode(source, 0.0f, null, LinkDirection.None, startSpline, startSpline.GetControlPointIndex(source));
            priorityQueue.Enqueue(node);

            while (!priorityQueue.IsEmpty())
            {
                AStarNode topNode = priorityQueue.Dequeue();

                if (topNode.point == target) // target reached
                {
                    // reconstruct path
                    while (topNode.point != source)
                    {
                        path.Add(topNode.ToPathLink());
                        topNode = topNode.prevNode;
                    }

                    path.Reverse();
                    return path;
                }

                Action<Pathfinding.ControlPoint, LinkDirection, CatmullRomSpline, int > processPoint = (pointToProcess, direction, parentSpline, index) =>
                {
                    if (pointToProcess != null && (ignoreVisitedCondition || pointToProcess.wasVisited) && !processed.ContainsKey(pointToProcess))
                    {
                        AStarNode newNode = toAStarNode(pointToProcess, target, topNode, direction, parentSpline, index);
                        AStarNode queuedNode = newNode;

                        if (priorityQueue.Contains(ref queuedNode, newNode.point)) // already in the queue
                        {
                            if(queuedNode.cost > newNode.cost) // if found a short cut
                            {
                                priorityQueue.Replace(queuedNode, newNode);
                            }
                        }
                        else
                        {
                            priorityQueue.Enqueue(newNode);
                        }
                    }
                };

                CatmullRomSpline spline = topNode.spline;
                int pointIndex = spline.GetControlPointIndex(topNode.point);
                ControlPoint left = spline.GetLeftPoint(pointIndex);
                processPoint(left, LinkDirection.Left, spline, pointIndex - 1);

                ControlPoint right = spline.GetRightPoint(pointIndex);
                processPoint(right, LinkDirection.Right, spline, pointIndex + 1);

                int linkedPointIndex = 0;
                ControlPoint vertical = spline.GetLinkedPoint(ref spline, ref linkedPointIndex, pointIndex);
                processPoint(vertical, LinkDirection.Vertical, spline, linkedPointIndex);

                processed.TryAdd(topNode.point, topNode);
            }

            // target cannot be reached!
            path.Clear();

            Debug.Log("Could not find a path from " + source + " to " + target);

            return path;
        }

        private static AStarNode toAStarNode(Pathfinding.ControlPoint currentPoint, Pathfinding.ControlPoint target, AStarNode prevNode, LinkDirection prevToThisDirection, CatmullRomSpline parentSpline, int pointIndex)
        {
            float teleportCost = 0.0f;
            float nodeCost = prevToThisDirection == LinkDirection.Vertical ? teleportCost : Mathf.Abs(currentPoint.localPos - prevNode.point.localPos);
            float remainingCost = (target.GetPosition() - currentPoint.GetPosition()).sqrMagnitude;

            float cost = prevNode.cost + nodeCost + remainingCost;

            AStarNode node = new AStarNode(currentPoint, cost, prevNode, prevToThisDirection, parentSpline, pointIndex);
            return node;
        }
    }
}