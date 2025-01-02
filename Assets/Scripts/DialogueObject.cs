using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    public struct GlobalCharacterReference
    {
        public string Key;
        public string Name;
        public GameObject GameObjectStaticReference;

        public GlobalCharacterReference(string key, string name, GameObject gameObjRef)
        {
            this.Key = key;
            this.Name = name;
            this.GameObjectStaticReference = gameObjRef;
        }
    }

    public class DialogueObject : MonoBehaviour
    {
        public string GlobalCharacterReferenceKey;
        public string GlobalCharacterReferenceName;

        public Vector2 DialoguePanelOffset = new Vector2(0.0f, 20.0f);
        public string ActiveNodeName; 

        private Yarn.Unity.DialogueRunner dialogueRunnerStaticRef;
        private GlobalCharacterReference globalCharacterReference;

        private void Start()
        {
            globalCharacterReference = new GlobalCharacterReference(GlobalCharacterReferenceKey, GlobalCharacterReferenceName, this.gameObject);
            dialogueRunnerStaticRef = GlobalData.Instance.DialogueRunnerStaticRef;

            GlobalData.Instance.RegisterGlobalCharacterReference(globalCharacterReference.Key, globalCharacterReference);
        }

        public void StartDialogue()
        {
            dialogueRunnerStaticRef.StartDialogue(ActiveNodeName);
        }
    }
}
