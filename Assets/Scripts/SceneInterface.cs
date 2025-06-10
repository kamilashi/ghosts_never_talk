using Library;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GNT
{
    [Serializable]
    public struct SceneStartData
    {
        public int startLayerIdx;
        public float positionOnLayer;
    }

    public enum LayerSwitchDirection
    {
        In,
        Out
    }

    [Serializable]
    public struct SceneReference
    {
        public int sceneHandle;
        public string sceneName;
    }

    [Serializable]
    public struct CameraConstraints
    {
        public float up;
        public float down;
        public float left;
        public float right;
    }

    public class SceneInterface : MonoBehaviour
    {
        [Header("Scene parameters")]
        public SceneStartData SceneStartData; // set in the inspector only
        public List<GroundLayer> GroundLayers; // set in the inspector only
        public CameraConstraints CameraConstraints;

        [SerializeField]
        private SceneReference sceneReference;


        void Awake()
        {
#if !UNITY_EDITOR
            ReloadSplines();
#endif

            Debug.Assert( gameObject.tag == "LevelScene", "The LevelScene tag of " + this.name + " is not set!");
        }

        void Update()
        {

        }

        public SceneStartData GetSceneStartData()
        {
            return SceneStartData;
        }
        public SceneReference GetSceneReference()
        {
            return sceneReference;
        }

        public void OnLoadInitialize()
        {
            sceneReference.sceneHandle = gameObject.scene.handle;
            sceneReference.sceneName = gameObject.scene.name;
            // delegate to load funcitonality and move to globalData
            for (int index = 0; index < GroundLayers.Count; index++)
            {
                GroundLayers[index].GroundLayerIndex = index;
            }
        }
        
        public GroundLayer GetGroundLayer(int targetIndex)
        {
            return GroundLayers[targetIndex];
        }

        public GroundLayer GetFartherOrThisGroundLayer(int currentIndex)
        {
            if (currentIndex < GroundLayers.Count - 1)
            {
                return GroundLayers[currentIndex + 1];
            }

            return GroundLayers[GroundLayers.Count - 1];
        }

        public GroundLayer GetCloserOrThisGroundLayer(int currentIndex)
        {
            if (currentIndex > 0)
            {
                return GroundLayers[currentIndex - 1];
            }

            return GroundLayers[0];
        }

        public void ShiftForegroundDown(int layerIndex, float distance, float duration)
        {
            GroundLayer layer = GroundLayers[layerIndex];

            Action onFinished = () =>
            {
                layer.SetShiftDownDistance(distance);
                layer.SetIsShiftedDown(true);
            };

            StartCoroutine(ShiftLayerVertically(layer, distance, -1.0f, duration,onFinished));

        }
        
        public void ShiftForegroundUp(int layerIndex, float duration)
        {
            GroundLayer layer = GroundLayers[layerIndex];

            Action onFinished = () =>
            {
                layer.SetShiftDownDistance(0.0f);
                layer.SetIsShiftedDown(false);
            };

            StartCoroutine(ShiftLayerVertically(layer, layer.GetShiftDownDistance(), 1.0f, duration, onFinished));
        }

        private IEnumerator ShiftLayerVertically(GroundLayer layer, float distance, float direction /*1 = Up, -1 = down*/, float duration, System.Action onCoroutineFinishedInteractAction)
        {
                Transform layerAssetsTransform = layer.LayerAssetsContainer.transform;
                Vector3 startPosition = layerAssetsTransform.position;
                Vector3 endPosition = startPosition;
                endPosition.y += direction * distance;
                float timer = 0.0f;

                do
                {
                    float progressLinear = timer / duration;
                    float progressTweened = SmoothingFuncitons.EaseOutCubic(progressLinear);

                    timer += Time.deltaTime;

                    Vector3 newPosition = Vector3.Lerp(startPosition, endPosition, progressTweened);
                    layerAssetsTransform.Translate(newPosition - layerAssetsTransform.position);

                    yield return null;
                }
                while (timer <= duration);


            onCoroutineFinishedInteractAction?.Invoke();
        }


        [ContextMenu("ReloadSplines")]
        public void ReloadSplines()
        {
            foreach (GroundLayer layer in GroundLayers)
            {
                layer.MovementSpline.TriggerOnValidate();
            }
        }

        public void OnLayerSwitch(int oldLayerIndex, int newLayerIndex)
        {
        }
    }
}