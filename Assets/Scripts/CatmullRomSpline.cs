using GNT;
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using System.Linq;

using static Unity.Burst.Intrinsics.X86.Avx;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.U2D;
#endif

namespace Pathfinding
{
    [Serializable]
    public struct InitializeSettings
    {
        public float distanceBetwPoints /*= 5.0f*/;
        public uint numControlPoints;
    }

    [Serializable]
    public class ControlPoint
    {
        public Transform transform;
        public GNT.SplinePointObject gameplayTrigger;
        public List<GNT.SplinePointObject> autoTriggers;
        public Vector3 position;
        public float localPos;
        public bool wasVisited;

        public ControlPoint(Transform transform, float localPos)
        {
            this.transform = transform;
            this.position = Vector3.zero;
            this.localPos = localPos; // distance from start
            this.wasVisited = false;
        }

        public ControlPoint(Vector3 position, float localPos)
        {
            this.position = position;
            this.transform = null;
            this.localPos = localPos; // distance from start
            this.wasVisited = false;
        }

        public List<GNT.SplinePointObject> GetAvailableAutoTriggers(float range, SplinePointObjectFaction userFaction)
        {
            List < GNT.SplinePointObject > triggersInRange = new List<SplinePointObject> ();

            foreach (GNT.SplinePointObject trigger in autoTriggers)
            {
                if(trigger.IsCorrectFaction(userFaction) && trigger.IsInDetectionRange(range))
                {
                    triggersInRange.Add(trigger);
                }
            }

            return triggersInRange;
        }

        public Vector3 GetPosition()
        {
            return transform ? transform.position : position;
        }

        public void SetLocalPos(float locPos)
        {
            this.localPos = locPos;
        }

        public float GetLocalPos()
        {
            return localPos;
        }

        public void Visit()
        { 
            if (!this.wasVisited)
            {
                this.wasVisited = true;
            }
        }
    }

    //Interpolation between points with a Catmull-Rom spline
    public class CatmullRomSpline : MonoBehaviour
    {
        public List<ControlPoint> controlPoints;
        public InitializeSettings initializeSettings;

        public float ObjectScanDistance = -1;

        [Range(1, 100)] public float resolution = 10;

        [SerializeField] private float totalLength;

        public Vector3 GetPositionOnSpline(ref SplineMovementData movementDataRef, float increment)
        {
            float newLocalPos = movementDataRef.positionOnSpline + increment;

            int point1Index = 0;
            for (int i = 0; i < controlPoints.Count - 1; i++)
            {
                if (movementDataRef.positionOnSpline >= controlPoints[i].GetLocalPos() && movementDataRef.positionOnSpline <= controlPoints[i + 1].GetLocalPos())
                {
                    point1Index = i;
                    float safetyError = 0.00f;

                    // clamp to end points:
                    if (newLocalPos < 0.0f)
                    {
                        newLocalPos = 0.0f + safetyError;
                    }

                    if (newLocalPos > totalLength)
                    {
                        newLocalPos = totalLength - safetyError;
                    }

                    movementDataRef.positionOnSpline = newLocalPos;

                    controlPoints[i].Visit(); // left
                    controlPoints[i + 1].Visit(); // right

                    movementDataRef.lastVisitedControlPoint = increment < 0.0f ? controlPoints[i] : controlPoints[i + 1];

                    break;
                }
            }

            float t = (movementDataRef.positionOnSpline - controlPoints[point1Index].GetLocalPos()) / (controlPoints[point1Index + 1].GetLocalPos() - controlPoints[point1Index].GetLocalPos());

            {
                List<SplinePointObject> availableRangedObjectsLastFrame = new List<SplinePointObject>();
                availableRangedObjectsLastFrame = movementDataRef.availableAutoTriggersThisFrame;

                ScanForSplinePointObjects(ref movementDataRef, point1Index, newLocalPos);

                TriggerRangedObjects(availableRangedObjectsLastFrame, movementDataRef.availableAutoTriggersThisFrame, ref movementDataRef);
            }

            return getPositionOnSplineSegment(point1Index, t);
        }

        private Vector3 getPositionOnSplineSegment(int point1Index, float t)
        {
            int startIdx = ClampListPos(point1Index - 1);
            int endIdx = ClampListPos(point1Index + 2);
            Vector3 p0 = controlPoints[startIdx].GetPosition();
            Vector3 p1 = controlPoints[point1Index].position;
            Vector3 p2 = controlPoints[point1Index + 1].GetPosition();
            Vector3 p3 = controlPoints[endIdx].GetPosition();

            return GetCatmullRomPosition(t, p0, p1, p2, p3);
        }

        public float GetLocalPositionOnSpline(int pointIndex)
        {
            return controlPoints[pointIndex].GetLocalPos();
        }
        public float GetTotalLength()
        {
            return totalLength;
        }

        public int GetControlPointIndex(ControlPoint point)
        {
            if (controlPoints.Contains(point))
            {
                return controlPoints.IndexOf(point);
            }

            return -1;
        }
        
        public ControlPoint GetControlPoint(int pointIndex)
        {
            return pointIndex < 0 || pointIndex >= controlPoints.Count? null : controlPoints[pointIndex];
        }

        public ControlPoint GetLeftPoint(int pointIndex)
        {
            return pointIndex <= 0 ? null : controlPoints[pointIndex - 1];
        }
        public ControlPoint GetRightPoint(int pointIndex)
        {
            return pointIndex >= controlPoints.Count - 1 ? null : controlPoints[pointIndex + 1];
        }
        public ControlPoint GetLinkedPoint(ref CatmullRomSpline linkedSplineRef, ref int linkedPointIndex, int pointIndex)
        {
            if (HasVerticalLink(controlPoints[pointIndex]))
            {
                InteractableTeleporter teleporter = (InteractableTeleporter) controlPoints[pointIndex].gameplayTrigger;
                linkedSplineRef = teleporter.TargetTeleporter.ContainingGroundLayer.MovementSpline;
                linkedPointIndex = teleporter.TargetTeleporter.GetSplinePointIndex();
                return linkedSplineRef.GetControlPoint(linkedPointIndex);
            }

            return null;
        }
        private ControlPoint GetLinkedPoint(int pointIndex)
        {
            Debug.Assert(HasVerticalLink(controlPoints[pointIndex]));

            InteractableTeleporter teleporter = (InteractableTeleporter) controlPoints[pointIndex].gameplayTrigger;
            CatmullRomSpline linkedSplineRef = teleporter.TargetTeleporter.ContainingGroundLayer.MovementSpline;
            int linkedPointIndex = teleporter.TargetTeleporter.GetSplinePointIndex();
            return linkedSplineRef.GetControlPoint(linkedPointIndex);
        }

        public bool HasVerticalLink(ControlPoint point)
        {  
            if(point.gameplayTrigger != null && point.gameplayTrigger.IsOfType(GNT.SplinePointObjectType.InteractableTeleporter))
            {
                InteractableTeleporter teleporter = (InteractableTeleporter) point.gameplayTrigger;

                return !teleporter.isReceiverOnly();
            }

            return false; 
        }

        public void TriggerRangedObjects(List<SplinePointObject> oldAvailableObjects, List<SplinePointObject> newAvailableObjects, ref SplineMovementData movementDataRef)
        {
            var newInactive = (oldAvailableObjects ?? Enumerable.Empty<SplinePointObject>()).Where(o => o != null);
            var newActive = (newAvailableObjects ?? Enumerable.Empty<SplinePointObject>()).Where(o => o != null);

            foreach (var trigger in newActive.Except(newInactive))
            {
                trigger.AutoTriggerInRange(ref movementDataRef);
            }

            foreach (var trigger in newInactive.Except(newActive))
            {
                trigger.AutoTriggerOutOfRange(ref movementDataRef);
            }
        }

        private void ScanForSplinePointObjects(ref SplineMovementData movementDataRef, int leftPointIdx, float positionOnSpline)
        {
            GNT.SplinePointObject nearestObject = null;

            int pointIdx = leftPointIdx;
            float scanDistance = ObjectScanDistance < 0 ? totalLength : ObjectScanDistance;
            float gameplayTriggerClosestDistance = float.MaxValue;
            float currentDistance = 0.0f;
            List<SplinePointObject> autoTriggerPoints = new List<SplinePointObject>();

            // scan left
            while (pointIdx >= 0 && currentDistance <= scanDistance)
            {
                ControlPoint scannedPoint = controlPoints[pointIdx];
                currentDistance = Math.Abs(scannedPoint.GetLocalPos() - positionOnSpline);

                // get the first gameplay trigger on the left
                if (nearestObject == null && scannedPoint.gameplayTrigger != null && scannedPoint.gameplayTrigger.IsInDetectionRange(currentDistance))
                {
                    nearestObject = scannedPoint.gameplayTrigger;
                    gameplayTriggerClosestDistance = currentDistance;
                }

                List<SplinePointObject> availableRangedAutoTriggers = scannedPoint.GetAvailableAutoTriggers(currentDistance, movementDataRef.splineUserFaction);
                if (availableRangedAutoTriggers.Count > 0)
                {
                    autoTriggerPoints.AddRange(availableRangedAutoTriggers);
                }

                pointIdx--;
            }

            currentDistance = 0.0f;
            pointIdx = leftPointIdx + 1;

            // scan right
            while (pointIdx < controlPoints.Count && currentDistance <= scanDistance)
            {
                ControlPoint scannedPoint = controlPoints[pointIdx];
                currentDistance = Math.Abs(scannedPoint.GetLocalPos() - positionOnSpline);

                // get the first gameplay trigger if it's closer than the one from the left
                if (scannedPoint.gameplayTrigger != null && currentDistance < gameplayTriggerClosestDistance && scannedPoint.gameplayTrigger.IsInDetectionRange(currentDistance))
                {
                    nearestObject = scannedPoint.gameplayTrigger;
                }

                List<SplinePointObject> availableRangedAutoTriggers = scannedPoint.GetAvailableAutoTriggers(currentDistance, movementDataRef.splineUserFaction);
                if (availableRangedAutoTriggers.Count > 0)
                {
                    autoTriggerPoints.AddRange(availableRangedAutoTriggers);
                }

                pointIdx++;
            }

            movementDataRef.availableGameplayTrigger = nearestObject;
            movementDataRef.availableAutoTriggersThisFrame = autoTriggerPoints;
        }

        private void OnValidate()
        {
            if (controlPoints == null || controlPoints.Count == 0)
            {
                return;
            }

            totalLength = 0.0f;


            System.Action<int, SplinePointObject> validatePoint = (splinePointIndex, splineObject) => 
            {
                splineObject.SetSplinePoint(splinePointIndex);
                splineObject.SetPosition(controlPoints[splinePointIndex].GetPosition());

                if (ObjectScanDistance > 0.0f && splineObject.DetectionRadius > ObjectScanDistance)
                {
                    Debug.LogWarning("Please increase the ObjectScanDistance of spline " + this.name + "!");
                }
            };

            controlPoints[0].SetLocalPos(0.0f);
            if (controlPoints[0].gameplayTrigger != null)
            {
                validatePoint(0, controlPoints[0].gameplayTrigger);
            }

            foreach (SplinePointObject autoTrigger in controlPoints[0].autoTriggers)
            {
                validatePoint(0, autoTrigger);
            }

            for (int i = 1; i < controlPoints.Count; i++)
            {
                Vector3 prevToThis = controlPoints[i].GetPosition() - controlPoints[i - 1].GetPosition();
                float distance = prevToThis.magnitude;
                totalLength += distance;
                controlPoints[i].SetLocalPos(totalLength);

                if (controlPoints[i].gameplayTrigger != null)
                {
                    validatePoint(i, controlPoints[i].gameplayTrigger);
                }

                foreach (SplinePointObject autoTrigger in controlPoints[i].autoTriggers)
                {
                    validatePoint(i, autoTrigger);
                }
            }
        }
        public void TriggerOnValidate()
        {
            OnValidate();
        }

#if UNITY_EDITOR
        [ContextMenu("Initialize")]
        void Initialize()
        {
            controlPoints.Clear();
            totalLength = 0.0f;

            uint numPoints = System.Math.Max(4u, initializeSettings.numControlPoints);

            ControlPoint startPoint = new ControlPoint(this.transform.position, 0.0f);
            controlPoints.Add(startPoint);

            for (int i = 1; i < numPoints; i++)
            {
                totalLength += initializeSettings.distanceBetwPoints;
                ControlPoint point = new ControlPoint(controlPoints[i - 1].GetPosition(), totalLength);
                point.position.x += initializeSettings.distanceBetwPoints;
                controlPoints.Add(point);
            }
        }

        [ContextMenu("AddPointsRight")]
        void AddPointsRight()
        {
            uint numPoints = initializeSettings.numControlPoints;
            int prevCount = controlPoints.Count;

            for (int i = 0; i < numPoints; i++)
            {
                totalLength += initializeSettings.distanceBetwPoints;
                ControlPoint point = new ControlPoint(controlPoints[prevCount + i - 1].GetPosition(), totalLength);
                point.position.x += initializeSettings.distanceBetwPoints;
                controlPoints.Add(point);
            }
        }


        [ContextMenu("AddPointsLeft")]
        void AddPointsLeft()
        {
            uint numPoints = initializeSettings.numControlPoints;

            List<ControlPoint> newControlPoints = new List<ControlPoint>();
            float newTotalLength = 0.0f;

            ControlPoint startPoint = new ControlPoint(controlPoints[0].position, 0.0f);
            startPoint.position.x -= numPoints * initializeSettings.distanceBetwPoints;
            newControlPoints.Add(startPoint);

            for (int i = 1; i < numPoints; i++)
            {
                newTotalLength += initializeSettings.distanceBetwPoints;
                ControlPoint point = new ControlPoint(newControlPoints[i - 1].GetPosition(), newTotalLength);
                point.position.x += initializeSettings.distanceBetwPoints;
                newControlPoints.Add(point);
            }

            totalLength += newTotalLength;
            controlPoints.InsertRange(0, newControlPoints);
        }

        [ContextMenu("ApplyRootPosition")]
        void ApplyRootPosition()
        {
            Vector3 oldRoot = controlPoints[0].position;
            controlPoints[0].position = transform.position;

            for (int i = 1; i < controlPoints.Count; i++)
            {
                Vector3 offset = controlPoints[i].position - oldRoot;
                controlPoints[i].position = transform.position + offset;
            }
        }

        void OnDrawGizmos()
        {
            if (controlPoints == null || controlPoints.Count == 0)
            {
                return;
            }

            const float heightOffset = 0.1f;

            for (int i = 0; i < controlPoints.Count; i++)
            {
                Gizmos.color = UnityEngine.Color.white;
                //Gizmos.DrawIcon(controlPoints[i].GetPosition(), "Solid_circle_red.png");
                Vector3 textPosigion = controlPoints[i].GetPosition();
                textPosigion.y += 0.5f;
                UnityEditor.Handles.Label(textPosigion, "p " + i);

                DisplayCatmullRomSpline(i, heightOffset);

                if (!HasVerticalLink(controlPoints[i]))
                {
                    continue;
                }

                // display link:
                ControlPoint linkedPoint = GetLinkedPoint(i);

                DisplayLink(controlPoints[i], linkedPoint, heightOffset);
            }
        }

        void DisplayCatmullRomSpline(int pointIdx, float heightOffset = 0.0f)
        {
            Vector3 p0 = controlPoints[ClampListPos(pointIdx - 1)].GetPosition();
            Vector3 p1 = controlPoints[pointIdx].position;
            Vector3 p2 = controlPoints[ClampListPos(pointIdx + 1)].GetPosition();
            Vector3 p3 = controlPoints[ClampListPos(pointIdx + 2)].GetPosition();

            //The start position of the line
            Vector3 lastPos = p1;
            lastPos.y += heightOffset;

            for (int i = 1; i <= resolution; i++)
            {
                float t = i * (1.0f / resolution);

                Vector3 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3);

                newPos.y += heightOffset;   

                Gizmos.DrawLine(lastPos, newPos);

                lastPos = newPos;
            }
        }

        void DisplayLink(ControlPoint from, ControlPoint to, float heightOffset = 0.0f)
        {
            bool isBilateral = (((InteractableTeleporter)to.gameplayTrigger).TargetTeleporter == (from.gameplayTrigger as InteractableTeleporter));

            if (isBilateral) 
            {
                Gizmos.color = UnityEngine.Color.green;
            }
            else
            {
                Gizmos.color = UnityEngine.Color.red;

                Vector3 direction = (from.position - to.position).normalized;

                Vector3 arrowHeadLeft = to.position + Vector3.up * heightOffset;
                arrowHeadLeft += direction;
                Vector3 arrowHeadRight = arrowHeadLeft;

                direction = Vector3.Cross(direction, Vector3.up);
                arrowHeadLeft += direction;
                arrowHeadRight -= direction;

                Gizmos.DrawLine(arrowHeadLeft, to.position);
                Gizmos.DrawLine(arrowHeadRight, to.position);
            }

            Gizmos.DrawLine(from.position, to.position);
        }
#endif

        //Clamp the list positions to allow looping
        int ClampListPos(int pointIdx)
        {
            if (pointIdx < 0)
            {
                pointIdx = 0;
            }

            else if (pointIdx > controlPoints.Count - 1)
            {
                pointIdx = controlPoints.Count - 1;
            }

            return pointIdx;
        }

        /*
        MIT License

        Copyright (c) 2020 Erik Nordeus

        Permission is hereby granted, free of charge, to any person obtaining a copy
        of this software and associated documentation files (the "Software"), to deal
        in the Software without restriction, including without limitation the rights
        to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        copies of the Software, and to permit persons to whom the Software is
        furnished to do so, subject to the following conditions:

        The above copyright notice and this permission notice shall be included in all
        copies or substantial portions of the Software.*/

        //Returns a position between 4 Vector3 with Catmull-Rom spline algorithm
        //http://www.iquilezles.org/www/articles/minispline/minispline.htm
        Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            //The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
            Vector3 a = 2f * p1;
            Vector3 b = p2 - p0;
            Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
            Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

            //The cubic polynomial: a + b * t + c * t^2 + d * t^3
            Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

            return pos;
        }
    }
}