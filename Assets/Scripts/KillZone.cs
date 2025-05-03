using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    public class KillZone : SplinePointObject
    {
        void Awake()
        {
            base.BaseAwakeSplinePointObject();
            splinePointObjectType = SplinePointObjectType.KillZone;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}