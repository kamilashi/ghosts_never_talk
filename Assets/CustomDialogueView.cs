using System;
using System.Collections;
using UnityEngine;

// Import the Yarn.Unity namespace so we get access to Yarn classes.
using Yarn.Unity;

namespace GNT
{
    public class CustomDialogueView : DialogueViewBase
    {
        public Vector3 WorldSpacePosition;
        // The amount of time that lines will take to appear.
        [SerializeField] private float characterAppearanceDelay = 0.01f;

        // The amount of time that lines will take to disappear.
        [SerializeField] private float characterDisappearanceDelay = 0.001f;

        // The text view to display the line of dialogue in.
        [SerializeField] TMPro.TextMeshProUGUI text;
        
        [SerializeField] TMPro.TextMeshProUGUI characterNameText;

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

        private string fullLineText;

        public void Start()
        {
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

            fullLineText = dialogueLine.TextWithoutCharacterName.Text;
            text.text = " ";

            GlobalCharacterReference charRef = GlobalData.Instance.GetGlobalCharacterByReference(dialogueLine.CharacterName);
            Vector2 worldSpaceOffset = charRef.GameObjectStaticReference.GetComponent<DialogueObject>().DialoguePanelOffset;
            Vector3 worldPosition = charRef.GameObjectStaticReference.transform.position;
            worldPosition.x += worldSpaceOffset.x;
            worldPosition.y += worldSpaceOffset.y;
            WorldSpacePosition = worldPosition;

            characterNameText.text = charRef.Name;

            advanceHandler = requestInterrupt;

            // Animate from zero to full scale, over the course of appearanceTime.
            currentAnimation = StartCoroutine(appearTextCoroutine(characterAppearanceDelay, fullLineText, false,
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
                ));
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

            // If we're in the middle of an animation, stop it.
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
                currentAnimation = null;
            }

            animateText(1.0f);

            Debug.Log($"{this.name} was interrupted while presenting {dialogueLine.TextID}");

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
                animateText(0.0f);
            };

            // Animate the box's scale from full to zero, and when we're done,
            // report that the dismissal is complete.
            currentAnimation = StartCoroutine(appearTextCoroutine(characterDisappearanceDelay, fullLineText, true,
                () =>
                {
                    // We're done animating! Signal that we're done.
                    advanceHandler = null;
                    Debug.Log($"{this.name} finished dismissing line");
                    currentAnimation = null;
                    onDismissalComplete();
                }
            ));
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

        private void animateText(float progress)
        {
            int charactersCount = Mathf.RoundToInt(progress * fullLineText.Length);
            string currentLineText = charactersCount == 0 ? " " : fullLineText.Substring(0, charactersCount);
            text.text = currentLineText;
        }

        private IEnumerator appearTextCoroutine(float charDelay, string fullText, bool decrement, Action onCompleteAction)
        {
            for (int i = 0; i <= fullText.Length; i++)
            {
                int charCount = decrement ? fullText.Length - i : i;

                float time = 0.0f; 
                string currentLineText = charCount == 0 ? " " : fullLineText.Substring(0, charCount);
                text.text = currentLineText;

                while (time < charDelay)
                {
                    time += Time.deltaTime;
                    yield return null;
                }
            }

            onCompleteAction?.Invoke();
        }
    }
}
