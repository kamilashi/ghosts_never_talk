using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    public class UIController : MonoBehaviour
    {

        [Header("Player Prompt")]
        public UIPlayerPrompt PlayerPrompt;
        public Vector2 offsetInPixels;

        // Start is called before the first frame update
        void Start()
        {
            PlayerPrompt.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            PlayerController PlayerRuntimeController = GlobalData.Instance.GetPlayerController();
            Interactable availableInteractable = PlayerRuntimeController.GetAvailableInteractable();
            if (availableInteractable != null)
            {
                //Vector2 screenSpacePos = GlobalData.Instance.GetActiveCamera().WorldToScreenPoint(PlayerRuntimeController.transform.position);
                //PlayerPrompt.transform.position = (screenSpacePos + offsetInPixels);
                PlayerPrompt.Set("Interact", PlayerRuntimeController.GetInteractKey());
                PlayerPrompt.gameObject.SetActive(true);
            }
            else
            {
                PlayerPrompt.gameObject.SetActive(false);
            }
        }
    }
}