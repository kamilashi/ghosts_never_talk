using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphics
{
    //[ExecuteInEditMode]
    public class ShaderPropertySetter : MonoBehaviour
    {
        private /*static*/ ShaderPropertySetter shaderPropertySetterInstance; // singleton


        public Camera cameraToSet;

/*
        [Header("Global Bottom Fade Parameters")]
        [Range(0, 1)]
        public float yOffset = 0.0f;
        [Range(0, 1)]
        public float fadeOutStart = 0.0f;
        [Range(0, 1)]
        public float fadeOutEnd = 0.0f;*/


        public static event Action SetLocalSpriteUVsEvent;

/*
        public static ShaderPropertySetter Instance
        {
            get
            {
                return shaderPropertySetterInstance;
            }
        }*/

        void Awake()
        {
            /*if (shaderPropertySetterInstance == null)
            {
                shaderPropertySetterInstance = gameObject.GetComponent<ShaderPropertySetter>();
            }*/

            //InitializeAllShaderParametersEvent += SetGlobalCameraParams;
            //InitializeAllShaderParametersEvent += SetGlobalBottomFadeParams;
        }

        void Update()
        {


            
        }

       void SetGlobalCameraParams()
        {
            //material.getSh
            Shader.SetGlobalFloat("_MainCamZPos", cameraToSet.transform.position.z);
            Shader.SetGlobalFloat("_MainCamNearPlane", cameraToSet.nearClipPlane);
            Shader.SetGlobalFloat("_MainCamFarPlane", cameraToSet.farClipPlane);
        }

/*
        void SetGlobalBottomFadeParams()
        {
            Shader.SetGlobalFloat("_BottomFade_FadeOutStart", fadeOutStart);
            Shader.SetGlobalFloat("_BottomFade_FadeOutEnd", fadeOutEnd);
            Shader.SetGlobalFloat("_BottomFade_YOffset", yOffset);
        }*/

        public void InitializeAllShaderParameters()
        {
            SetGlobalCameraParams();
            SetLocalSpriteUVsEvent?.Invoke();
        }

        public void SetLocalUVs()
        {
            SetLocalSpriteUVsEvent?.Invoke();
        }
        public void SetGlobalCameraParameters()
        {
            SetGlobalCameraParams();
        }
    }

}