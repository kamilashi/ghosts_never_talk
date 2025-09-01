using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    [ExecuteAlways]
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
            /*float right_screen_offset = 10;
           float element_width = 200;
           float element_height = 30;
           float vertical_interval = 35;
           float screep_pos_y_from_top = 35;
           int ui_element_no = 0;
           float screen_width = Screen.width;

          if (GUI.Button(new Rect(screen_width - element_width - right_screen_offset, screep_pos_y_from_top + ui_element_no++ * vertical_interval, element_width, element_height), "Set Camera Params"))
           {
               // call event
               shaderPropertySetter.SetGlobalCameraParameters();
           }


                       if (GUI.Button(new Rect(screen_width - element_width - right_screen_offset, screep_pos_y_from_top + ui_element_no++ * vertical_interval, element_width, element_height), "Set Local UVs"))
                       {
                           // call event
                           shaderPropertySetter.SetLocalUVs();
                       }

                       if (GUI.Button(new Rect(screen_width - element_width - right_screen_offset, screep_pos_y_from_top + ui_element_no++ * vertical_interval, element_width, element_height), "Set Global Sprite Bottom Fade"))
                       {
                           shaderPropertySetter.SetGlobalSpriteBottomFadeParameters();
                       }

                       if (GUI.Button(new Rect(screen_width - element_width - right_screen_offset, screep_pos_y_from_top + ui_element_no++ * vertical_interval, element_width, element_height), "Set Global Sprite Distance Fade"))
                       {
                           shaderPropertySetter.SetGlobalSpriteDistanceFadeParameters();
                       }*/
        }
    }
}