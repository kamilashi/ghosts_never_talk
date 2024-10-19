using System.Collections;
using System.Collections.Generic;

namespace Library
{
    public static class SmoothingFuncitons
    {
        public static float ApproachReferenceLinear(float input, float reference, float speed)
        {
            float direction = System.Math.Sign(reference - input);
            float output = input + direction * speed;
            output = direction > 0 ? System.Math.Min(output, reference) : System.Math.Max(output, reference);
            return output;
        }
        
        /// <summary>
        /// Based on magnitude
        /// </summary>
        /// <param name="input"> value to lerp</param>
        /// <param name="reference">target value</param>
        /// <param name="riseLerpValue">lag when rising</param>
        /// <param name="fallLerpValue">lag when falling</param>
        /// <returns></returns>
        public static UnityEngine.Vector2 LerpToReferenceNonLinear(UnityEngine.Vector2 input, UnityEngine.Vector2 reference, float riseLerpValue, float fallLerpValue)
        {
            UnityEngine.Vector2 output;
            if (UnityEngine.Vector2.SqrMagnitude(reference) > UnityEngine.Vector2.SqrMagnitude(input))
            {
                output = UnityEngine.Vector2.Lerp(input, reference, riseLerpValue);
            }
            else
            {
                output = UnityEngine.Vector2.Lerp(input, reference, fallLerpValue);
            }

            return output;
        }

        public static UnityEngine.Vector2 ApproachReferenceNonLinear(UnityEngine.Vector2 input, UnityEngine.Vector2 reference, UnityEngine.Vector2 speed, float riseLerpValue, float fallLerpValue)
        {
            UnityEngine.Vector2 output;
            if (UnityEngine.Vector2.SqrMagnitude(reference) > UnityEngine.Vector2.SqrMagnitude(input))
            {
                output = UnityEngine.Vector2.Lerp(input, reference, riseLerpValue);
            }
            else
            {
                output = UnityEngine.Vector2.Lerp(input, reference, fallLerpValue);
            }

            return output;
        }
    }
}