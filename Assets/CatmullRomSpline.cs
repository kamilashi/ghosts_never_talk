using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 
 * CatmullRomSpline scprit is a variation of the CatmullRomSpline implementation
 *  provided by Erik Nordeus under the following license:
 *  
 *  
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

[Serializable]
public struct InitializeSettings
{
    public float distanceBetwPoints;
    public uint numControlPoints;
}

[Serializable]
public class ControlPoint
{
    public Transform transform;
    public Vector3 position;
    public float localPos;

    public ControlPoint(Transform transform, float localPos)
    {
        this.transform = transform;
        this.position = Vector3.zero;
        this.localPos = localPos; // distance from start
    }
    
    public ControlPoint(Vector3 position, float localPos)
    {
        this.position = position;
        this.transform = null;
        this.localPos = localPos; // distance from start
    }

    public Vector3 getPosition()
    {
        return transform ? transform.position : position;
    }

    public void setLocalPos(float locPos)
    {
        this.localPos = locPos;
    } 
    
    public float getLocalPos()
    {
        return localPos;
    }
}

//Interpolation between points with a Catmull-Rom spline
public class CatmullRomSpline : MonoBehaviour
{
    public List<ControlPoint> controlPoints;
    public InitializeSettings initializeSettings;

    [Range(1, 100)] public float resolution = 10;

    [SerializeField] private float totalLength;

    [ContextMenu("Initialize")]
    void Initialize()
    {
        controlPoints.Clear();
        totalLength = 0.0f;

        uint numPoints = System.Math.Max(4u, initializeSettings.numControlPoints);

        ControlPoint startPoint = new ControlPoint(this.transform.position, 0.0f);
        controlPoints.Add (startPoint);

        for (int i = 1; i < numPoints; i++)
        {
            totalLength += initializeSettings.distanceBetwPoints;
            ControlPoint point = new ControlPoint(controlPoints[i - 1].getPosition(), totalLength);
            point.position.x += initializeSettings.distanceBetwPoints;
            controlPoints.Add(point);
        }
    }

    public Vector3 GetPositionOnSpline(ref float localPosRef, float increment)
    {
        float newLocalPos = localPosRef + increment;

        int point1Index = 0;
        for (int i = 0; i < controlPoints.Count-1; i++)
        {
            if(localPosRef >= controlPoints[i].getLocalPos() && localPosRef < controlPoints[i+1].getLocalPos())
            {
                point1Index = i;

                if (newLocalPos >= 0 && newLocalPos <= totalLength)
                {
                    localPosRef = newLocalPos;
                }

                break;
            }
        }

        float t = (localPosRef - controlPoints[point1Index].getLocalPos()) / (controlPoints[point1Index + 1].getLocalPos() - controlPoints[point1Index].getLocalPos());

        return getPositionOnSplineSegment(point1Index, t);
    }

   private Vector3 getPositionOnSplineSegment(int point1Index, float t)
    {
        int startIdx = ClampListPos(point1Index - 1);
        int endIdx = ClampListPos(point1Index + 2);
        Vector3 p0 = controlPoints[startIdx].getPosition();
        Vector3 p1 = controlPoints[point1Index].position;
        Vector3 p2 = controlPoints[point1Index + 1].getPosition();
        Vector3 p3 = controlPoints[endIdx].getPosition();

        return GetCatmullRomPosition(t, p0, p1, p2, p3);
    }

    public float GetLocalPositionOnSpline(int pointIndex)
    {
        return controlPoints[pointIndex].getLocalPos();
    }

    private void OnValidate()
    {
        //Vector3 startToEnd = controlPoints[controlPoints.Count-1].getPosition() - controlPoints[0].getPosition();
        //totalDistance = startToEnd.magnitude;
        totalLength = 0.0f;

        for (int i = 1; i < controlPoints.Count; i++)
        {
            Vector3 prevToThis = controlPoints[i].getPosition() - controlPoints[i-1].getPosition();
            float distance = prevToThis.magnitude;
            totalLength += distance;
            controlPoints[i].setLocalPos(totalLength);
        }
    }

    void OnDrawGizmos()
    {
        if(controlPoints.Count == 0)
        {
            return;
        }

        Gizmos.color = Color.white;

        Gizmos.DrawIcon(controlPoints[0].getPosition(), "Solid_circle_red.png");
        Gizmos.DrawIcon(controlPoints[controlPoints.Count - 1].getPosition(), "Solid_circle_red.png");

        //Draw the Catmull-Rom spline between the points
        for (int i = 0; i < controlPoints.Count; i++)
        {
            Gizmos.DrawIcon(controlPoints[i].getPosition(), "Solid_circle_red.png");

            DisplayCatmullRomSpline(i);
        }
    }

    void DisplayCatmullRomSpline(int pointIdx)
    {
        Vector3 p0 = controlPoints[ClampListPos(pointIdx - 1)].getPosition();
        Vector3 p1 = controlPoints[pointIdx].position;
        Vector3 p2 = controlPoints[ClampListPos(pointIdx + 1)].getPosition();
        Vector3 p3 = controlPoints[ClampListPos(pointIdx + 2)].getPosition();

        //The start position of the line
        Vector3 lastPos = p1;

        for (int i = 1; i <= resolution; i++)
        {
            float t = i * (1.0f / resolution);

            Vector3 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3);

            Gizmos.DrawLine(lastPos, newPos);

            lastPos = newPos;
        }
    }

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
