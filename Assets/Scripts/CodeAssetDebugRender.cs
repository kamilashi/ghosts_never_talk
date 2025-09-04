using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    [ExecuteAlways]
    public class CodeAssetDebugRender : MonoBehaviour
    {
        public enum RenderShape
        {
            LineX,
            CrossXY,
            Bounds
        }

        public RenderShape Shape = RenderShape.LineX;
        public float Extents = 5.0f;

        static bool IsEnabledGlobal;
        public bool IsEnabledLocal = false;

        void Start()
        {

        }

        [ContextMenu("ToggleVisibility")]
        void ToggleGlobalVisibility()
        {
            IsEnabledGlobal = !IsEnabledGlobal;
        }

        [ExecuteAlways]
        void Update()
        {
            if(!IsEnabledGlobal && !IsEnabledLocal)
            { 
                return; 
            }

            switch(Shape)
            {
                case RenderShape.LineX:
                    {
                        Vector3 leftEnd = new Vector3();
                        Vector3 rightEnd = new Vector3();
                        rightEnd = (transform.position);
                        leftEnd = rightEnd;
                        rightEnd.x += Extents;
                        leftEnd.x -= Extents;

                        Debug.DrawLine(leftEnd, rightEnd, Color.magenta, Time.deltaTime, false);
                    }
                    break;
                case RenderShape.CrossXY:
                    {
                        Vector3 lineStart = new Vector3();
                        Vector3 lineEnd = new Vector3();
                        lineEnd = (transform.position);
                        lineStart = lineEnd;
                        lineEnd.x += Extents;
                        lineStart.x -= Extents;
                        Debug.DrawLine(lineStart, lineEnd, Color.magenta, Time.deltaTime, false);

                        lineEnd = (transform.position);
                        lineStart = lineEnd;
                        lineEnd.y += Extents;
                        lineStart.y -= Extents;
                        Debug.DrawLine(lineStart, lineEnd, Color.magenta, Time.deltaTime, false);
                    }
                    break;
                case RenderShape.Bounds:
                    {
                        Vector3 endPoint = new Vector3();
                        Vector3 offset = new Vector3(0.0f, 1.0f, 0.0f);
                        endPoint = (transform.position);
                        endPoint.x -= Extents;
                        Debug.DrawLine(endPoint, endPoint + offset, Color.magenta, Time.deltaTime, false);

                        endPoint = (transform.position);
                        endPoint.x += Extents;
                        Debug.DrawLine(endPoint, endPoint + offset, Color.magenta, Time.deltaTime, false);
                    }
                    break;
                default:
                    {
                        Debug.LogError("render Shape behavior is not defined for !" + Shape.ToString());
                    }
                    break;
            }
        }
    }
}