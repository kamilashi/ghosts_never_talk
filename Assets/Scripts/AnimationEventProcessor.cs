using System.Collections;
using System.Collections.Generic;

namespace ProcessingHelpers
{
    public delegate void Notify();  // delegate
    public abstract class DurationAnimationEvent
    {
        public event Notify DurationEnded; // event

        public void InvokeDurationEnded()
        {
            DurationEnded?.Invoke();
        }
            // protected virtual void CountDown();
    }

    public class TimeDurationAnimationEvent : DurationAnimationEvent
    {
        float timeDurationCountdown; // in seconds

        public TimeDurationAnimationEvent(float duration)
        {
            this.timeDurationCountdown = duration;
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

        public FrameDurationAnimationEvent(int duration)
        {
            this.frameDurationCountdown = duration;
        }

        public bool CountDown()
        {
            frameDurationCountdown -= 1;
            return frameDurationCountdown <= 0;
        }
    }

    public class AnimationEventProcessor
    {
        List<TimeDurationAnimationEvent> timeDurationEventBuffer;
        List<FrameDurationAnimationEvent> frameDurationEventBuffer;

        public void Run(float timeStep)
        {
            foreach (TimeDurationAnimationEvent animEvent in timeDurationEventBuffer)
            {
               if(animEvent.CountDown(timeStep))
                {
                    animEvent.InvokeDurationEnded();
                }
            }
            
            foreach (FrameDurationAnimationEvent animEvent in frameDurationEventBuffer)
            {
               if(animEvent.CountDown())
                {
                    animEvent.InvokeDurationEnded();
                }
            }
        }

        public TimeDurationAnimationEvent RegisterDurationEvent(float durationInSeconds)
        {
            TimeDurationAnimationEvent animationEvent = new TimeDurationAnimationEvent(durationInSeconds);

            return animationEvent;
        }
        public FrameDurationAnimationEvent RegisterDurationEvent(int durationInFrames)
        {
            FrameDurationAnimationEvent animationEvent = new FrameDurationAnimationEvent(durationInFrames);

            return animationEvent;
        }
    }
}