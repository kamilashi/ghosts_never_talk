using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProcessingHelpers;
// this is persistent

namespace GNT
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager globalDataInstance; // singleton

        public SceneInterface StartSceneStaticRef; // read only, set in the inspector only, invisible to other scripts
        public Camera MainCameraStaticRef; // read only, set in the inspector only, invisible to other scripts
        public PlayerController PlayerControllerStaticRef; // read only, reference needs to be set in the inspector, visible too other scripts
        public CharacterMovement PlayerMovementStaticRef; // read only, reference needs to be set in the inspector, visible too other scripts
        public CustomDialogueView DialogueViewStaticRef; // read only, reference needs to be set in the inspector, visible too other scripts
        public Yarn.Unity.DialogueRunner DialogueRunnerStaticRef; // read only, reference needs to be set in the inspector, visible too other scripts


        public SceneInterface ActiveSceneDynamicRef; //read + write only from the owner script #todo : maybe should be handled by the scene manager,global reference to which should be stored here - should be visible to other scripts

        public Dictionary<string, GlobalCharacterReference> GlobalCharacterRefDictionary;
        public AnimationEventProcessor AnimationEventProcessorInstance;

        public static event Action OnSceneLoadFinishEvent;

        public static GameManager Instance
        {
            get
            {
                return globalDataInstance;
            }
        }


        void Awake()
        {
            if (globalDataInstance == null)
            {
                globalDataInstance = gameObject.GetComponent<GameManager>();
            }

            // Hack, this should be in the project settings:
            Physics2D.queriesStartInColliders = false;
            AnimationEventProcessorInstance = new AnimationEventProcessor();

            GlobalCharacterRefDictionary = new Dictionary<string, GlobalCharacterReference>();

            // until we have loading and save data:
            ActiveSceneDynamicRef = StartSceneStaticRef;
            ActiveSceneDynamicRef.OnLoadInitialize();
        }

        void Start()
        {
            OnSceneLoadFinishEvent?.Invoke();

            SceneStartData sceneStartData = StartSceneStaticRef.GetSceneStartData();
            PlayerMovementStaticRef.SwitchToLayer(StartSceneStaticRef.GetGroundLayer(sceneStartData.startLayerIdx), sceneStartData.positionOnLayer);
        }

        void Update()
        {
            // #todo: move to some game processor/simulation script?
            AnimationEventProcessorInstance.Run(Time.deltaTime);
        }

        public Camera GetActiveCamera()
        {
            return MainCameraStaticRef;
        }

        public PlayerController GetPlayerController()
        {
            return PlayerControllerStaticRef;
        }
        public CharacterMovement GerPlayerMovement()
        {
            return PlayerMovementStaticRef;
        }

        public void RegisterGlobalCharacterReference(string key, GlobalCharacterReference value)
        {
            if(!GlobalCharacterRefDictionary.ContainsKey(key))
            {
                GlobalCharacterRefDictionary.Add(key, value);
            }
            else
            {
                Debug.LogError("Failed to register GCR with key, value = " + key + "," + value + " \nThe key already exists");
            }
        }

        public GlobalCharacterReference GetGlobalCharacterByReference(string key)
        {
            return GlobalCharacterRefDictionary[key];
        }
    }
}