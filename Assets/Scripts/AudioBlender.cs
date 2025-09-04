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
    public class SoundEventData
    {
        public string eventPath;
        public string parameterName;
        public float interpolationDuration;
        public float startValue = -1;
        public float endValue;

        public int eventId;
    }

    public class AudioBlender : SplinePointObject
    {
        [Header("Audio Bledner")]

        public List<SoundEventData> onEnterRangeBlendEvents;
        public List<SoundEventData> onExitRangeBlendEvents;

        public List<SoundEventData> directionalBlends;

        //private List<Coroutine> interpolateSoundParameterCoroutines;

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
            for (int i = 0; i < onEnterRangeBlendEvents.Count; i++)
            {
                SoundEventData data = onEnterRangeBlendEvents[i];
                data.eventId = GameManager.Instance.FetchSoundbanEventId(data.eventPath);
            }

            for (int i = 0; i < onExitRangeBlendEvents.Count; i++)
            {
                SoundEventData data = onExitRangeBlendEvents[i];
                data.eventId = GameManager.Instance.FetchSoundbanEventId(data.eventPath);
            }

            for (int i = 0; i < directionalBlends.Count; i++)
            {
                SoundEventData data = directionalBlends[i];
                data.eventId = GameManager.Instance.FetchSoundbanEventId(data.eventPath);
            }
        }

        public override void AutoTriggerInRange(ref SplineMovementData movementDataRef)
        {
            Debug.Log("Triggered on in range");
            foreach (SoundEventData soundData in onEnterRangeBlendEvents)
            {
                StartOneShotBlend(soundData);
            }

            foreach (SoundEventData soundData in directionalBlends)
            {
                StartContinuousBlend(soundData, ref movementDataRef);
            }
        }

        public override void AutoTriggerOutOfRange(ref SplineMovementData movementDataRef)
        {
            Debug.Log("Triggered on out of range");

            foreach (SoundEventData soundData in onExitRangeBlendEvents)
            {
                StartOneShotBlend(soundData);
            }
        }

        private void StartOneShotBlend(SoundEventData soundData)
        {
            EventInstance eventInstance = GameManager.Instance.GetSoundbankEventInstance(soundData.eventId);
            float startValue = soundData.startValue;
            
            if(startValue < 0)
            {
                eventInstance.getParameterByName(soundData.parameterName, out startValue);
            }

            StartCoroutine(InterpolateParameterOneShot(startValue, soundData.endValue, soundData.interpolationDuration, eventInstance, soundData.parameterName));
        }

        private void StartContinuousBlend(SoundEventData soundData, ref SplineMovementData movementDataRef)
        {
            EventInstance eventInstance = GameManager.Instance.GetSoundbankEventInstance(soundData.eventId);

            StartCoroutine(InterpolateParameterContinuous(soundData.startValue, soundData.endValue, movementDataRef, eventInstance, soundData.parameterName));
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
