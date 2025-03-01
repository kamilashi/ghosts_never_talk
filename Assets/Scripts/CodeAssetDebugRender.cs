using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    [ExecuteAlways]
    public class CodeAssetDebugRender : MonoBehaviour
    {
        static bool isEnabled;

        public enum RenderShape
        {
            LineX,
            CrossXY
        }

        public RenderShape Shape = RenderShape.LineX;
        public float Extents = 5.0f;
        public static bool IsEnabled;
        void Start()
        {

        }

        [ContextMenu("ToggleVisibility")]
        void ToggleVisibility()
        {
            isEnabled = !isEnabled;
        }

        [ExecuteAlways]
        void Update()
        {
            if(!isEnabled)
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
                default:
                    {
                        Debug.LogError("render Shape behavior is not defined for !" + Shape.ToString());
                    }
                    break;
            }
        }
    }
}