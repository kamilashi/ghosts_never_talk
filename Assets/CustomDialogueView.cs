using System;
using System.Collections;
using UnityEngine;

// Import the Yarn.Unity namespace so we get access to Yarn classes.
using Yarn.Unity;

namespace GNT
{
    public class CustomDialogueView : DialogueViewBase
    {
        // The amount of time that lines will take to appear.
        [SerializeField] private float appearanceTime = 0.5f;

        // The amount of time that lines will take to disappear.
        [SerializeField] private float disappearanceTime = 0.5f;

        // The text view to display the line of dialogue in.
        [SerializeField] TMPro.TextMeshProUGUI text;
        
        [SerializeField] TMPro.TextMeshProUGUI characterNameText;

        // The game object that should animate in and out.
        [SerializeField] RectTransform container;

        // If this is true, then the line view will not automatically report that
        // it's done showing a line, and will instead wait for InterruptLine to be
        // called (which happens when UserRequestedViewAdvancement is called.)
        [SerializeField] private bool waitForInput;

        // The current coroutine that's playing out a scaling animation. When this
        // is not null, we're in the middle of an animation.
        Coroutine currentAnimation;

        // Stores a reference to the method to call when the user wants to advance
        // the line.
        Action advanceHandler = null;

        private float lineAnimateProgress;

        // Sets the scale of the container view.
        private float LineAnimateProgress
        {
            set => lineAnimateProgress = value;
        }

        public void Start()
        {
            LineAnimateProgress = 0;
        }

        // RunLine receives a localized line, and is in charge of displaying it to
        // the user. When the view is done with the line, it should call
        // onDialogueLineFinished.
        //
        // Unless the line gets interrupted, the Dialogue Runner will wait until all
        // views have called their onDialogueLineFinished, before telling them to
        // dismiss the line and proceeding on to the next one. This means that if
        // you want to keep a line on screen for a while, simply don't call
        // onDialogueLineFinished until you're ready.
        public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
        {
            // We shouldn't do anything if we're not active.
            if (gameObject.activeInHierarchy == false)
            {
                onDialogueLineFinished();
                return;
            }

            Debug.Log($"{this.name} running line {dialogueLine.TextID}");

            //Scale = 0;
            text.text = dialogueLine.Text.Text;
            characterNameText.text = dialogueLine.CharacterName;

            advanceHandler = requestInterrupt;

            
            // Animate from zero to full scale, over the course of appearanceTime.
            currentAnimation = this.Tween(
                0f, 1f,
                appearanceTime,
                (from, to, t) => LineAnimateProgress = Mathf.Lerp(from, to, t),
                () => // on complete Action
                {
                    // We're done animating!
                    Debug.Log($"{this.name} finished presenting {dialogueLine.TextID}");
                    currentAnimation = null;

                    // Should we wait for input, or immediately report that we're
                    // done?
                    if (waitForInput)
                    {
                        advanceHandler = requestInterrupt;
                    }
                    else
                    {
                        advanceHandler = null;
                        onDialogueLineFinished();
                    }
                }
                );
        }

        
        public override void InterruptLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
        {
            if (gameObject.activeInHierarchy == false)
            {
                onDialogueLineFinished();
                return;
            }

            // If we get an interrupt, we need to skip to the end of our
            // presentation as quickly as possible

            // there's nothing we can do to be faster, so we'll do nothing here.
            advanceHandler = null;

            Debug.Log($"{this.name} was interrupted while presenting {dialogueLine.TextID}");

            // If we're in the middle of an animation, stop it.
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
                currentAnimation = null;
            }

            // Skip to the end of the presentation by setting our scale to 100%.
            LineAnimateProgress = 1f;

            // Indicate that we've finished presenting the line.
            onDialogueLineFinished();
        }

        // DismissLine is called when the dialogue runner has instructed us to get
        // rid of the line. This is our view's opportunity to do whatever animations
        // we need to to get rid of the line. When we're done, we call
        // onDismissalComplete. When all line views have called their
        // onDismissalComplete, the dialogue runner moves on to the next line.
        public override void DismissLine(Action onDismissalComplete)
        {
            if (gameObject.activeInHierarchy == false)
            {
                // This line view isn't active; it should immediately report that
                // it's finished dismissing.
                onDismissalComplete();
                return;
            }

            Debug.Log($"{this.name} dismissing line");

            // If we have an animation running, stop it.
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
                currentAnimation = null;
            }

            // If we receive an advance signal WHILE dismissing the line, skip the
            // rest of the animation entirely and report that our dismissal is
            // complete.
            advanceHandler = () =>
            {
                if (currentAnimation != null)
                {
                    StopCoroutine(currentAnimation);
                    currentAnimation = null;
                }
                advanceHandler = null;
                onDismissalComplete();
                LineAnimateProgress = 0f;
            };

            // Animate the box's scale from full to zero, and when we're done,
            // report that the dismissal is complete.
            currentAnimation = this.Tween(
                1f, 0f,
                disappearanceTime,
                (from, to, t) => LineAnimateProgress = Mathf.Lerp(from, to, t),
                () =>
                {
                    // We're done animating! Signal that we're done.
                    advanceHandler = null;
                    Debug.Log($"{this.name} finished dismissing line");
                    currentAnimation = null;
                    onDismissalComplete();
                }
            );
        }

        // RunOptions is called when the Dialogue Runner needs to show options. It
        // receives an array containing the options, and a method to run when an
        // option has been selected. 
        //
        // This view only displays lines, not options. (We've found it useful to
        // break up the line views based on role - so, one view for lines, another
        // view for options.)
        //
        // public override void RunOptions(DialogueOption[] dialogueOptions,
        // Action<int> onOptionSelected)
        // {
        // }

        public override void UserRequestedViewAdvancement()
        {
            // Invoke our 'advance line' handler, which (depending on what we're
            // currently doing) will be a signal to interrupt the line, stop the
            // current animation, or do nothing.
            advanceHandler?.Invoke();
        }
    }

    // A class that adds extension methods to MonoBehaviour that allows for
    // animating a property over time.
    public static class TweenExtensions
    {

        // Defines a type of function that can be called over time, taking a start
        // and end value, and a 'factor' value between 0 and 1.
        public delegate void TweenFunction<T>(T from, T to, float factor);

        // Starts and returns a coroutine that begins running an animation from
        // 'from' to 'to' over 'time' seconds, calling the 'handler' function every
        // frame, and calling the optional 'onComplete' method when it's done.
        //
        // This method is an 'extension' method: because it's in a static class, is
        // static itself, and the first parameter has the 'this' keyword, C# will
        // act as though this 'Tween' method is a part of the existing
        // 'MonoBehaviour' class. In effect, we're adding a new method to the
        // Unity-provided class!
        //
        // Additionally, we're making this method use a type parameter 'T'. Because
        // the tweening system doesn't do anything with the 'from' and 'to' values
        // itself, it doesn't actually matter what type they are, so we'll leave
        // this open to interpretation. This means you could use this method to
        // tween between two values of any type - integers, floats, Vector3's, even
        // strings! It's up to the 'handler' method to decide what to do at every
        // frame.
        public static Coroutine Tween<T>(this MonoBehaviour behaviour, T from, T to, float time, TweenFunction<T> handler, System.Action onComplete = null)
        {

            // Coroutines run on specific MonoBehaviours, so we'll tell the
            // 'behaviour' to start running it our HandleTween coroutine.
            return behaviour.StartCoroutine(HandleTween(from, to, time, handler, onComplete));
        }

        // The coroutine that actually performs the work of animating the change.
        private static IEnumerator HandleTween<T>(T from, T to, float time, TweenFunction<T> handler, System.Action onComplete)
        {
            // How much time has elapsed since we started this change.
            var timeElapsed = 0f;

            // Loop until 'time' seconds have elapsed.
            while (timeElapsed < time)
            {

                // Our handler function expects the 'factor' parameter to be between
                // zero and one, where zero is the start of the animation and one is
                // the end of the animation.
                //
                // We'll get this value by dividing timeElapsed by total time, and
                // then clamping the result to between 0 and 1.
                float factor = Mathf.Clamp01(timeElapsed / time);

                // We have everything we need - call the handler function to perform
                // whatever specific work we need to do.
                handler(from, to, factor);

                // Update our elapsed time.
                timeElapsed += Time.deltaTime;

                // Yield the coroutine so that other game work can be done. (This is
                // very important! If you forget this, Unity will freeze up.)
                yield return null;
            }

            // We've reached the end of our animation. Tidy up by calling the
            // handler one last time, with a value of 1. (It's very unlikely that
            // the 'factor' variable we calcuate above would reach an even value of
            // 1.0, and certain animations would look strange if they end early,
            // like an object's transparency.) To deal with this, we'll call it one
            // last time with the final value.

            handler(from, to, 1f);

            // Finally, if we had an on-complete method to call, call it now.
            onComplete?.Invoke();
        }
    }
}
