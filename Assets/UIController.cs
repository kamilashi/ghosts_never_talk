using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    public class UIController : MonoBehaviour
    {
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

            PlayerPromptStaticRef.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            Interactable availableInteractable = playerConrtollerStaticRef.GetAvailableInteractable();
            if (availableInteractable != null)
            {
                //Vector2 screenSpacePos = GlobalData.Instance.GetActiveCamera().WorldToScreenPoint(PlayerRuntimeController.transform.position);
                //PlayerPrompt.transform.position = (screenSpacePos + offsetInPixels);
                PlayerPromptStaticRef.Set("Interact", playerConrtollerStaticRef.GetInteractKey());
                PlayerPromptStaticRef.gameObject.SetActive(true);
            }
            else if(dialogueRunnerStaticRef.IsDialogueRunning)
            {
                PlayerPromptStaticRef.Set("Next", playerConrtollerStaticRef.GetAdvanceDialogueKey());
                PlayerPromptStaticRef.gameObject.SetActive(true);
            }
            else
            {
                PlayerPromptStaticRef.gameObject.SetActive(false);
            }
        }
    }
}