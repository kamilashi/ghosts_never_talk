using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphics
{
    // should be per scene!
    //[ExecuteInEditMode]
    public class ShaderPropertySetter : MonoBehaviour
    {
        private /*static*/ ShaderPropertySetter shaderPropertySetterInstance; // singleton


        public Camera cameraToSet;

        [Header("Sprite Global Bottom Fade")]
        [Range(0, 1)]
        public float yCutOff = 0.0f;
        [Range(0, 1)]
        public float fadeOutStart = 0.12f;
        [Range(0, 1)]
        public float fadeOutEnd = 0.0f;

        [Header("Sprite Global Distance Fade")]
        [Range(0, 1)]
        public float intensity = 0.8f;
        [Range(0, 1)]
        public float farClipMofidier = 0.0f;
        [Range(0, 1)]
        public float nearClipModifier = 0.0f;


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

        void SetGlobalBottomFadeParams()
        {
            Shader.SetGlobalFloat("_Sprite_BottomFade_FadeOutStart", fadeOutStart);
            Shader.SetGlobalFloat("_Sprite_BottomFade_FadeOutEnd", fadeOutEnd);
            Shader.SetGlobalFloat("_Sprite_BottomFade_YOffset", yCutOff);
        }

        void SetGlobalDistanceFadeParams()
        {
            Shader.SetGlobalFloat("_Sprite_DistanceFade_Intensity", intensity);
            Shader.SetGlobalFloat("_Sprite_DistanceFade_FarClipModifier", farClipMofidier);
            Shader.SetGlobalFloat("_Sprite_DistanceFade_NearClipModifier", nearClipModifier);
        }

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
        public void SetGlobalSpriteBottomFadeParameters()
        {
            SetGlobalBottomFadeParams();
        }
        public void SetGlobalSpriteDistanceFadeParameters()
        {
            SetGlobalDistanceFadeParams();
        }
    }

}