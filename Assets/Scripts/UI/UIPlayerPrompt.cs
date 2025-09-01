using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace GNT
{
    public class UIPlayerPrompt : MonoBehaviour
    {
        public TMP_Text PromptActionText;
        public TMP_Text PromptActionIcon; 

        void Start()
        {

        }

        void Update()
        {

        }

        public void Set(string promptActionText, string promptActionIcon)
        {
            PromptActionText.text = promptActionText;
            PromptActionIcon.text = promptActionIcon;
        }
    }
}

