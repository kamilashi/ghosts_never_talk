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
public struct ControlPoint
{
    public Transform transform;
    public Vector3 position;

    public ControlPoint(Transform transform)
    {
        this.transform = transform;
        this.position = Vector3.zero;
    }
    
    public ControlPoint(Vector3 position)
    {
        this.position = position;
        this.transform = null;
    }

    public Vector3 getPosition()
    {
        return transform ? transform.position : position;
    }
}

//Interpolation between points with a Catmull-Rom spline
public class CatmullRomSpline : MonoBehaviour
{
    //Has to be at least 4 points
    public List<ControlPoint> controlPoints;

    [Range(1, 100)]
    public float resolution = 10;

    public InitializeSettings initializeSettings;

    [ContextMenu("Initialize")]
    void Initialize()
    {
        uint numPoints = System.Math.Max(4u, initializeSettings.numControlPoints);

        ControlPoint startPoint = new ControlPoint(this.transform.position);
        controlPoints.Add (startPoint);

        for (int i = 1; i < numPoints; i++)
        {
            ControlPoint point = new ControlPoint(controlPoints[i - 2].getPosition());
            point.position.x += initializeSettings.distanceBetwPoints;
            controlPoints.Add(point);
        }
    }

    //Display without having to press play
    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        Gizmos.DrawIcon(controlPoints[0].getPosition(), "Solid_circle_red.png");

        //Draw the Catmull-Rom spline between the points
        for (int i = 1; i < controlPoints.Count; i++)
        {
            Gizmos.DrawIcon(controlPoints[i].getPosition(), "Solid_circle_red.png");

            DisplayCatmullRomSpline(i);
        }
    }

    //Display a spline between 2 points derived with the Catmull-Rom spline algorithm
    void DisplayCatmullRomSpline(int pointIdx)
    {
        //The 4 points we need to form a spline between p1 and p2
        Vector3 p0 = controlPoints[ClampListPos(pointIdx - 1)].getPosition();
        Vector3 p1 = controlPoints[pointIdx].position;
        Vector3 p2 = controlPoints[ClampListPos(pointIdx + 1)].getPosition();
        Vector3 p3 = controlPoints[ClampListPos(pointIdx + 2)].getPosition();

        //The start position of the line
        Vector3 lastPos = p1;

        //How many times should we loop?
        //int loops = Mathf.FloorToInt(1f / resolution);

        for (int i = 1; i <= resolution; i++)
        {
            //Which t position are we at?
            float t = i * 1.0f / resolution;

            //Find the coordinate between the end points with a Catmull-Rom spline
            Vector3 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3);

            //Draw this line segment
            Gizmos.DrawLine(lastPos, newPos);

            //Save this pos so we can draw the next line segment
            lastPos = newPos;
        }
    }

    //Clamp the list positions to allow looping
    int ClampListPos(int pos)
    {
        if (pos < 0)
        {
            pos = controlPoints.Count - 1;
        }

        if (pos > controlPoints.Count)
        {
            pos = 1;
        }
        else if (pos > controlPoints.Count - 1)
        {
            pos = 0;
        }

        return pos;
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
