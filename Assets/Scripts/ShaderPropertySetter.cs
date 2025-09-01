using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphics
{
    // should be per scene!
    [ExecuteInEditMode]
    public class ShaderPropertySetter : MonoBehaviour
    {
        public Camera cameraToSet;

/*
        [Header("Sprite Global Bottom Fade")]
        [Range(0, 1)]
        public float yCutOff = 0.0f;
        [Range(0, 1)]
        public float fadeOutStart = 0.12f;
        [Range(0, 1)]
        public float fadeOutEnd = 0.0f;*/

        [Header("Sprite Global Distance Fade")]
        [Range(0, 1)]
        public float intensity = 0.8f;
        public float farClipMofidier = 0.0f;
        public float nearClipModifier = 0.0f;

        [Header("Sprite Global Lighting")]
        [Range(0, 1)]
        public float nightMode = 0.0f;

        public static event Action SetLocalSpriteUVsEvent;

        public GameObject spritesContainer;
        public RenderTexture localSpriteUVRenderTexture;
        public string outputTextureName; // final output
        public ComputeShader localUVConverterComputeShader;

        void Awake()
        {
        }

        [ExecuteInEditMode]
        void Update()
        {
           // SetGlobalBottomFadeParams();
            SetGlobalDistanceFadeParams();
            SetGlobalLightingParams();
        }

        void SetGlobalCameraParams()
        {
            Shader.SetGlobalFloat("_MainCamZPos", cameraToSet.transform.position.z);
            Shader.SetGlobalFloat("_MainCamNearPlane", cameraToSet.nearClipPlane);
            Shader.SetGlobalFloat("_MainCamFarPlane", cameraToSet.farClipPlane);
        }

/*
        void SetGlobalBottomFadeParams()
        {
            Shader.SetGlobalFloat("_Sprite_BottomFade_FadeOutStart", fadeOutStart);
            Shader.SetGlobalFloat("_Sprite_BottomFade_FadeOutEnd", fadeOutEnd);
            Shader.SetGlobalFloat("_Sprite_BottomFade_YOffset", yCutOff);
        }*/

        void SetGlobalDistanceFadeParams()
        {
            Shader.SetGlobalFloat("_Sprite_DistanceFade_Intensity", intensity);
            Shader.SetGlobalFloat("_Sprite_DistanceFade_FarClipModifier", farClipMofidier);
            Shader.SetGlobalFloat("_Sprite_DistanceFade_NearClipModifier", nearClipModifier);
        }

        void SetGlobalLightingParams()
        {
            Shader.SetGlobalFloat("_Sprite_Lighting_NightMode", nightMode);
        }

        public void InitializeAllShaderParameters()
        {
            SetGlobalCameraParams();
            SetLocalSpriteUVsEvent?.Invoke();
        }

        public void SetAllLocalUVs()
        {
            SetLocalSpriteUVsEvent?.Invoke();
        }
        public void SetGlobalCameraParameters()
        {
            SetGlobalCameraParams();
        }
/*
        public void SetGlobalSpriteBottomFadeParameters()
        {
            SetGlobalBottomFadeParams();
        }*/
        public void SetGlobalSpriteDistanceFadeParameters()
        {
            SetGlobalDistanceFadeParams();
        }

        [ContextMenu("WriteLocalUVDataFromSpritesYCoordOnly")]
        private void WriteLocalUVDataFromSpritesYCoordOnly()
        {
            foreach (Transform sprite in spritesContainer.transform)
            {
                if (sprite.GetComponent<LocalUVConverter>() != null)
                {
                    LocalUVConverter uvConverterComponent = sprite.GetComponent<LocalUVConverter>();
                    uvConverterComponent.WriteLocalUVCoordYIntoRenderTexture(ref localSpriteUVRenderTexture, ref localUVConverterComputeShader);
                }
            }

            Library.TextureWriter.SaveRenderTextureToFile(localSpriteUVRenderTexture, outputTextureName, true, Library.TextureWriter.SaveTextureFileFormat.PNG);
        }
        
        [ContextMenu("WriteLocalUVDataFromSprites")]
        private void WriteLocalUVDataFromSprites()
        {
            foreach (Transform sprite in spritesContainer.transform)
            {
                if (sprite.GetComponent<LocalUVConverter>() != null)
                {
                    LocalUVConverter uvConverterComponent = sprite.GetComponent<LocalUVConverter>();
                    uvConverterComponent.WriteLocalUVDataIntoRenderTexture(ref localSpriteUVRenderTexture, ref localUVConverterComputeShader);
                }
            }

            Library.TextureWriter.SaveRenderTextureToFile(localSpriteUVRenderTexture, outputTextureName, true, Library.TextureWriter.SaveTextureFileFormat.PNG);
        }

        [ContextMenu("ClearRenderTexture")]
        private void ClearRenderTexture()
        {
            int clearTextKernel = localUVConverterComputeShader.FindKernel("clearRenderTexture");
            localUVConverterComputeShader.SetTexture(clearTextKernel, "localSpriteUVRenderTexture", localSpriteUVRenderTexture);

            localUVConverterComputeShader.Dispatch(clearTextKernel, 1, localSpriteUVRenderTexture.height, 1);


            Library.TextureWriter.SaveRenderTextureToFile(localSpriteUVRenderTexture, outputTextureName, true, Library.TextureWriter.SaveTextureFileFormat.PNG);
        }
    }
}