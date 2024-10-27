using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    [ExecuteInEditMode]
    public class MateralPropertySetter : MonoBehaviour
    {
       // public List<Material> setCameraForMaterials;
        public Camera cameraToSet;
        //public depthGradient
        //private Material thisMaterial;

        // #todo Implement singleton!!!

        void Awake()
        {
            {
                //material.getSh
                Shader.SetGlobalFloat("_MainCamZPos", cameraToSet.transform.position.z);
                Shader.SetGlobalFloat("_MainCamNearPlane", cameraToSet.nearClipPlane);
                Shader.SetGlobalFloat("_MainCamFarPlane", cameraToSet.farClipPlane);
            }
        }

        void Update()
        {

        }
    }

}