using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    public class DebugGUI : MonoBehaviour
    {
        public Graphics.ShaderPropertySetter shaderPropertySetter;

        void Awake()
        {

        }

        void Update()
        {

        }

        [ExecuteAlways]
        private void OnGUI()
        {
            OnDebugGui();
        }


        [ExecuteAlways]
        private void OnDebugGui()
        {
            float element_width = 100;
            float element_height = 30;
            float vertical_interval = 35;
            float screep_pos_y_from_top = 35;
            int ui_element_no = 0;
            float screen_width = Screen.width;

            if (GUI.Button(new Rect(screen_width - 110, screep_pos_y_from_top + ui_element_no++ * vertical_interval, element_width, element_height), "Set All Shader Params"))
            {
                // call event
                shaderPropertySetter.InitializeAllShaderParameters();
            }
        }
    }
}