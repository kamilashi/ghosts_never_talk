using FMOD.Studio;
using FMODUnity;
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
        public float leftValue;
        public float endValue;

        public int eventId;
    }

    public class AudioBlender : SplinePointObject
    {
        [Header("Audio Bledner")]

        public List<SoundEventData> onEnterRangeBlendEvents;
        public List<SoundEventData> onExitRangeBlendEvents;

        public List<SoundEventData> directionalBlends;
/*
        private void Awake()
        {
            if(!GameManager.isLoaded)
            {
                return;
            };


            splinePointObjectType = SplinePointObjectType.AutoTriggerRange;

            GameManager.OnSceneLoadFinishEvent += GetEventIds;

            base.BaseAwakeSplinePointObject();
        }

        private void OnDestroy()
        {
            GameManager.OnInitialized -= GetEventIds;
        }*/

        [ContextMenu("GetEventIds")]
        private void GetEventIds()
        {
            for (int i = 0; i < onEnterRangeBlendEvents.Count; i++)
            {
                SoundEventData data = onEnterRangeBlendEvents[i];
                data.eventId = GameManager.Instance.GetSoundbanEventId(data.eventPath);
            }

            for (int i = 0; i < onExitRangeBlendEvents.Count; i++)
            {
                SoundEventData data = onExitRangeBlendEvents[i];
                data.eventId = GameManager.Instance.GetSoundbanEventId(data.eventPath);
            }

            for (int i = 0; i < directionalBlends.Count; i++)
            {
                SoundEventData data = directionalBlends[i];
                data.eventId = GameManager.Instance.GetSoundbanEventId(data.eventPath);
            }
        }

        public override void AutoTriggerInRange()
        {
            Debug.Log("Triggered on in range");
            foreach (SoundEventData soundData in onEnterRangeBlendEvents)
            {
                HandleOneShotSoundEvent(soundData);
            }

            //emitter.Play();
        }

        public override void AutoTriggerOutOfRange()
        {
            Debug.Log("Triggered on out of range");

            foreach (SoundEventData soundData in onExitRangeBlendEvents)
            {
                HandleOneShotSoundEvent(soundData);
            }
        }

        private void HandleOneShotSoundEvent(SoundEventData soundData)
        {
            //EventInstance eventInstance = GameManager.Instance.GetSoundbankEvent(soundData.eventId);
            EventInstance eventInstance = GameManager.Instance.SoundbankEvents[GameManager.Instance.SoundbankEventIDs[soundData.eventPath]];
            float startValue;
            eventInstance.getParameterByName(soundData.parameterName, out startValue);
            //eventInstance.stop(0);

            StartCoroutine(InterpolateParameter(startValue, soundData.endValue, soundData.interpolationDuration, eventInstance, soundData.parameterName));
        }

        private IEnumerator InterpolateParameter(float start, float target, float duration, EventInstance soundEventInstance, string parameterName)
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
    }
}
