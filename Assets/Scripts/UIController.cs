using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    public class UIController : MonoBehaviour
    {
        [Header("Dialogue Panel")]
        public CustomDialogueView DialoguePanelStaticRef;
        public RectTransform DialoguePanelRectTransformStaticRef;
        public float StartEndAnimateSpeed;
        
        [Header("Player Prompt")]
        public UIPlayerPrompt PlayerPromptStaticRef;
        public Vector2 offsetInPixels;

        private Yarn.Unity.DialogueRunner dialogueRunnerStaticRef;
        private PlayerController playerConrtollerStaticRef;

        // Start is called before the first frame update
        void Start()
        {
            playerConrtollerStaticRef = GlobalData.Instance.GetPlayerController();
            dialogueRunnerStaticRef = GlobalData.Instance.DialogueRunnerStaticRef;

            (DialoguePanelStaticRef.transform as RectTransform).localScale = new Vector3(0.0f, 0.0f, 0.0f);

            PlayerPromptStaticRef.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (dialogueRunnerStaticRef.IsDialogueRunning)
            {
                Vector3 screenSpacePos = GlobalData.Instance.GetActiveCamera().WorldToScreenPoint(DialoguePanelStaticRef.WorldSpacePosition);
                (DialoguePanelStaticRef.transform as RectTransform).position = new Vector2(screenSpacePos.x, screenSpacePos.y);

                PlayerPromptStaticRef.Set("Next", playerConrtollerStaticRef.GetAdvanceDialogueKey());
                PlayerPromptStaticRef.gameObject.SetActive(true);
            }
            else
            {
                Interactable availableInteractable = playerConrtollerStaticRef.GetAvailableInteractable();
                if (availableInteractable != null)
                {
                    PlayerPromptStaticRef.Set("Interact", playerConrtollerStaticRef.GetInteractKey());
                    PlayerPromptStaticRef.gameObject.SetActive(true);
                }
                else
                {
                    PlayerPromptStaticRef.gameObject.SetActive(false);
                }
            }
        }

        private IEnumerator DialoguePanelAnimateCoroutine(RectTransform animatedTransform, float startAnimatedValue, float endAnimatedValue)
        {
            animatedTransform.localScale = new Vector3(startAnimatedValue, startAnimatedValue, startAnimatedValue);
            float progress = 0.0f;
            float currentAnimatedValue = startAnimatedValue;

            while (progress < 1.0f)
            {
                float diff = endAnimatedValue - currentAnimatedValue;
                float direction = Mathf.Sign(diff);
                currentAnimatedValue += direction * Mathf.Min(Time.deltaTime * StartEndAnimateSpeed, Mathf.Abs(diff));

                animatedTransform.localScale = new Vector3(currentAnimatedValue, currentAnimatedValue, currentAnimatedValue);

                progress = (currentAnimatedValue - startAnimatedValue) / (endAnimatedValue - startAnimatedValue);
                yield return null;
            }
        }

        public void OnDialogueStart()
        {
            StartCoroutine(DialoguePanelAnimateCoroutine(DialoguePanelStaticRef.transform as RectTransform, 0.0f, 1.0f));
        }
        
        public void OnDialogueEnd()
        {
            StartCoroutine(DialoguePanelAnimateCoroutine(DialoguePanelStaticRef.transform as RectTransform, 1.0f, 0.0f));
        }
    }
}