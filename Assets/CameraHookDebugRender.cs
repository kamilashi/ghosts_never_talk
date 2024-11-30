using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    [ExecuteAlways]
    public class CameraHookDebugRender : MonoBehaviour
    {
        public float extents = 5.0f;
        void Start()
        {

        }

        [ExecuteAlways]
        void Update()
        {
            Vector3 leftEnd = new Vector3();
            Vector3 rightEnd = new Vector3();
            rightEnd = (transform.position);
            leftEnd = rightEnd;
            rightEnd.x += extents;
            leftEnd.x -= extents;

            Debug.DrawLine(leftEnd, rightEnd, Color.magenta, Time.deltaTime, false);
        }
    }
}