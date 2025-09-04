using FMOD.Studio;
using FMODUnity;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace GNT
{
    [Serializable]
    public abstract class ParameterBlendData
    {
        public string eventPath;
        public string parameterName;

        [SerializeField]
        private int eventId;
        public void SetId(int id) { this.eventId = id; }
        public int GetId() { return this.eventId; }
    }

    [Serializable]
    public class OneShotBlendData : ParameterBlendData
    {
        public float interpolationDuration;
        public float startValue;
        public bool starFromCurrent;
        public float endValue;
    }

    [Serializable]
    public class ContinuousBlendData : ParameterBlendData
    {
        public float leftValue;
        public float rightValue;
    }

    public class AudioBlender : SplinePointObject
    {
        [Header("Audio Blender")]

        public List<OneShotBlendData> onEnterRangeBlends;
        public List<OneShotBlendData> onExitRangeBlends;
        public List<ContinuousBlendData> directionalBlends;

        private void Awake()
        {
            splinePointObjectType = SplinePointObjectType.AutoTriggerRange;

            GameManager.OnSceneLoadFinishEvent += FetchEventIds;

            base.BaseAwakeSplinePointObject();
        }
        private void OnDestroy()
        {
            GameManager.OnSceneLoadFinishEvent -= FetchEventIds;
        }

        private void FetchEventIds()
        {
            for (int i = 0; i < onEnterRangeBlends.Count; i++)
            {
                ParameterBlendData data = onEnterRangeBlends[i];
                data.SetId(GameManager.Instance.FetchSoundbanEventId(data.eventPath));
            }

            for (int i = 0; i < onExitRangeBlends.Count; i++)
            {
                ParameterBlendData data = onExitRangeBlends[i]; 
                data.SetId(GameManager.Instance.FetchSoundbanEventId(data.eventPath));
            }

            for (int i = 0; i < directionalBlends.Count; i++)
            {
                ParameterBlendData data = directionalBlends[i];
                data.SetId(GameManager.Instance.FetchSoundbanEventId(data.eventPath));
            }
        }

        public override void AutoTriggerInRange(ref SplineMovementData movementDataRef)
        {
            Debug.Log("Triggered on in range");
            foreach (OneShotBlendData soundData in onEnterRangeBlends)
            {
                StartOneShotBlend(soundData);
            }

            foreach (ContinuousBlendData soundData in directionalBlends)
            {
                StartContinuousBlend(soundData, ref movementDataRef);
            }
        }

        public override void AutoTriggerOutOfRange(ref SplineMovementData movementDataRef)
        {
            Debug.Log("Triggered on out of range");

            foreach (OneShotBlendData soundData in onExitRangeBlends)
            {
                StartOneShotBlend(soundData);
            }
        }

        private void StartOneShotBlend(OneShotBlendData soundData)
        {
            EventInstance eventInstance = GameManager.Instance.GetSoundbankEventInstance(soundData.GetId());
            float startValue = soundData.startValue;
            
            if(startValue < 0)
            {
                eventInstance.getParameterByName(soundData.parameterName, out startValue);
            }

            StartCoroutine(InterpolateParameterOneShot(startValue, soundData.endValue, soundData.interpolationDuration, eventInstance, soundData.parameterName));
        }

        private void StartContinuousBlend(ContinuousBlendData soundData, ref SplineMovementData movementDataRef)
        {
            EventInstance eventInstance = GameManager.Instance.GetSoundbankEventInstance(soundData.GetId());

            StartCoroutine(InterpolateParameterContinuous(soundData.leftValue, soundData.rightValue, movementDataRef, eventInstance, soundData.parameterName));
        }

        private IEnumerator InterpolateParameterOneShot(float start, float target, float duration, EventInstance soundEventInstance, string parameterName)
        {
            float current = start;
            float progress = 0.0f;
            float time = 0.0f;
            while(time < duration)
            {
                time += Time.deltaTime;
                progress = time / duration;
                progress = Math.Clamp(progress, 0.0f, 1.0f);
                current = Mathf.Lerp(start, target, progress);
                soundEventInstance.setParameterByName(parameterName, current);
                yield return null;
            }
        }

        private IEnumerator InterpolateParameterContinuous(float start, float target, SplineMovementData movementDataRef, EventInstance soundEventInstance, string parameterName)
        {
            float current = start;
            float progress = 0.0f;
            ControlPoint splinePoint = ContainingGroundLayer.MovementSpline.GetControlPoint(pointIndex);
            float leftPoint = Math.Max(splinePoint.localPos - DetectionRadius, 0.0f);
            float rightPoint = Math.Min(splinePoint.localPos + DetectionRadius, ContainingGroundLayer.MovementSpline.GetTotalLength());

            while (progress >= 0f && progress <= 1.0f)
            {
                progress = (movementDataRef.positionOnSpline - leftPoint) / (rightPoint - leftPoint);
                current = Mathf.Lerp(start, target, Math.Clamp(progress, 0.0f, 1.0f));
                soundEventInstance.setParameterByName(parameterName, current);

                yield return null;
            }
        }
    }
}
