using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace ProcessingHelpers
{
    public delegate void OnFinishedCallbackDelegate();

    public abstract class DurationAnimationEvent
    {
        public OnFinishedCallbackDelegate onFinishedCallbackDelegate;

        public void InvokeDurationEnded()
        {
            onFinishedCallbackDelegate?.Invoke();
        }
        protected void RegisterOnFinishedCallback(OnFinishedCallbackDelegate callBack)
        {
            onFinishedCallbackDelegate = callBack;
        }
    }

    public class TimeDurationAnimationEvent : DurationAnimationEvent
    {
        float timeDurationCountdown; // in seconds

        public TimeDurationAnimationEvent(float duration, OnFinishedCallbackDelegate callBack)
        {
            this.timeDurationCountdown = duration;
            RegisterOnFinishedCallback(callBack);
        }
        public bool CountDown(float timeSTep)
        {
            timeDurationCountdown -= timeSTep;
            return timeDurationCountdown <= 0.0f;
        }
    }

    public class FrameDurationAnimationEvent : DurationAnimationEvent
    {
        int frameDurationCountdown; // in seconds

        public FrameDurationAnimationEvent(int duration, OnFinishedCallbackDelegate callBack)
        {
            frameDurationCountdown = duration;
            RegisterOnFinishedCallback(callBack);
        }

        public bool CountDown()
        {
            frameDurationCountdown -= 1;
            return frameDurationCountdown <= 0;
        }
    }

    public class AnimationEventProcessor
    {
        List<TimeDurationAnimationEvent> timeDurationEventBuffer = new List<TimeDurationAnimationEvent>();
        List<FrameDurationAnimationEvent> frameDurationEventBuffer = new List<FrameDurationAnimationEvent>();

        public void Run(float timeStep)
        {
            List<int> removeEventIndecesQueue = new List<int>();

            for (int idx = 0; idx < timeDurationEventBuffer.Count; idx++)
            {
               TimeDurationAnimationEvent animEvent = timeDurationEventBuffer[idx];
               if (animEvent.CountDown(timeStep))
                {
                    //UnityEngine.Debug.Log("event end!");
                    animEvent.InvokeDurationEnded();
                    removeEventIndecesQueue.Add(idx);
                }
            }

            foreach(int index in removeEventIndecesQueue)
            {
                timeDurationEventBuffer.RemoveAt(index);
            }

            removeEventIndecesQueue.Clear();

            for (int idx = 0; idx < frameDurationEventBuffer.Count; idx++)
            {
               FrameDurationAnimationEvent animEvent = frameDurationEventBuffer[idx];
                if (animEvent.CountDown())
                {
                    animEvent.InvokeDurationEnded();
                    removeEventIndecesQueue.Add(idx);
                }
            }

            foreach (int index in removeEventIndecesQueue)
            {
                frameDurationEventBuffer.RemoveAt(index);
            }
        }

        public void RegisterDurationEvent(float durationInSeconds, OnFinishedCallbackDelegate eventHandlerDelegate)
        {
#if UNITY_EDITOR
            Assert.IsTrue(durationInSeconds > 0.0f, "Duration events must have a non-zero duration!");
#endif
            TimeDurationAnimationEvent animationEvent = new TimeDurationAnimationEvent(durationInSeconds, eventHandlerDelegate);
            timeDurationEventBuffer.Add(animationEvent);
        }

        public void RegisterDurationEvent(int durationInFrames, OnFinishedCallbackDelegate eventHandlerDelegate)
        {
            FrameDurationAnimationEvent animationEvent = new FrameDurationAnimationEvent(durationInFrames, eventHandlerDelegate);
            frameDurationEventBuffer.Add(animationEvent);
        }
    }
}