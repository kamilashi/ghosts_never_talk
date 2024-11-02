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

    public static class TextWriter
   {
        public static void WriteTextToFile(string content, string resourceFolder, string fileNameNoExt)
        {
            string wholePath = "Assets/Resources/" + resourceFolder + fileNameNoExt + ".txt";
            System.IO.StreamWriter writer = new System.IO.StreamWriter(wholePath, false);
            writer.WriteLine(content);
            writer.Close();
            //Re-import the file to update the reference in the editor
            UnityEditor.AssetDatabase.ImportAsset(wholePath);

            //Print the text from the file
            UnityEngine.TextAsset asset = (UnityEngine.TextAsset) UnityEngine.Resources.Load(resourceFolder + fileNameNoExt);
            UnityEngine.Debug.Log(asset.text);
        }
        public static string LoadTextFromFile(string fileResourcePath, string fileNameNoExt)
        {
            string content = "";

            UnityEngine.TextAsset asset = (UnityEngine.TextAsset) UnityEngine.Resources.Load("Assets/Resources/" + fileResourcePath + fileNameNoExt);
            content = asset.text;
            return content;
        }

        public static void SaveVec3(UnityEngine.Vector3 serializable, string folderPath, string fileNameNoExt)
        {
            string jsonText = UnityEngine.JsonUtility.ToJson(serializable);

            WriteTextToFile(jsonText, folderPath, fileNameNoExt);
        }

        public static UnityEngine.Vector3 LoadVec3(string folderPath, string fileNameNoExt)
        {
            string jsonText = LoadTextFromFile(folderPath, fileNameNoExt);

            UnityEngine.Vector3 vector3 = UnityEngine.JsonUtility.FromJson<UnityEngine.Vector3>(jsonText);

            UnityEngine.Debug.Log("Loaded " + vector3.ToString());
            return vector3;
        }
    }
}