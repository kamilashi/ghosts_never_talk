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
    }
}