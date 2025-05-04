using Library;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public class SceneInterface : MonoBehaviour
    {

        [SerializeField]
        private SceneStartData sceneStartData; // set in the inspector only

        [SerializeField]
        private List<GroundLayer> groundLayers;

        void Awake()
        {
#if !UNITY_EDITOR
            ReloadSplines();
#endif
        }

        void Update()
        {

        }

        public SceneStartData GetSceneStartData()
        {
            return sceneStartData;
        }

        public void OnLoadInitialize()
        {
            // delegate to load funcitonality and move to globalData
            for (int index = 0; index < groundLayers.Count; index++)
            {
                groundLayers[index].GroundLayerIndex = index;
            }
        }
        
        public GroundLayer GetGroundLayer(int targetIndex)
        {
            return groundLayers[targetIndex];
        }

        public GroundLayer GetFartherOrThisGroundLayer(int currentIndex)
        {
            if (currentIndex < groundLayers.Count - 1)
            {
                return groundLayers[currentIndex + 1];
            }

            return groundLayers[groundLayers.Count - 1];
        }

        public GroundLayer GetCloserOrThisGroundLayer(int currentIndex)
        {
            if (currentIndex > 0)
            {
                return groundLayers[currentIndex - 1];
            }

            return groundLayers[0];
        }

        public void ShiftForegroundDown(int layerIndex, float distance, float duration)
        {
            GroundLayer layer = groundLayers[layerIndex];

            Action onFinished = () =>
            {
                layer.SetShiftDownDistance(distance);
                layer.SetIsShiftedDown(true);
            };

            StartCoroutine(ShiftLayerVertically(layer, distance, -1.0f, duration,onFinished));

        }
        
        public void ShiftForegroundUp(int layerIndex, float duration)
        {
            GroundLayer layer = groundLayers[layerIndex];

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
            foreach (GroundLayer layer in groundLayers)
            {
                layer.MovementSpline.TriggerOnValidate();
            }
        }

        public void OnLayerSwitch(int oldLayerIndex, int newLayerIndex)
        {
        }
    }
}