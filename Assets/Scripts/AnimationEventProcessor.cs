using System.Collections;
using System.Collections.Generic;

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
            foreach (TimeDurationAnimationEvent animEvent in timeDurationEventBuffer)
            {
               if(animEvent.CountDown(timeStep))
                {
                    // #todo : handle collection modified case 
                    animEvent.InvokeDurationEnded();
                    timeDurationEventBuffer.Remove(animEvent);
                    if(timeDurationEventBuffer.Count == 0)
                    {
                        break;
                    }
                }
            }
            
            foreach (FrameDurationAnimationEvent animEvent in frameDurationEventBuffer)
            {
               if(animEvent.CountDown())
                {
                    // #todo : handle collection modified case 
                    animEvent.InvokeDurationEnded();
                    frameDurationEventBuffer.Remove(animEvent);
                    if (timeDurationEventBuffer.Count == 0)
                    {
                        break;
                    }
                }
            }
        }

        public void RegisterDurationEvent(float durationInSeconds, OnFinishedCallbackDelegate eventHandlerDelegate)
        {
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