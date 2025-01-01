using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProcessingHelpers;
// this is persistent

namespace GNT
{
    public class GlobalData : MonoBehaviour
    {
        private static GlobalData globalDataInstance; // singleton

        public SceneInterface StartSceneStaticRef; // read only, set in the inspector only, invisible to other scripts
        public Camera MainCameraStaticRef; // read only, set in the inspector only, invisible to other scripts
        public PlayerController PlayerControllerStaticRef; // read only, reference needs to be set in the inspector, visible too other scripts
        public CustomDialogueView DialogueViewStaticRef; // read only, reference needs to be set in the inspector, visible too other scripts


        public SceneInterface ActiveSceneDynamicRef; //read + write only from the owner script #todo : maybe should be handled by the scene manager,global reference to which should be stored here - should be visible to other scripts

        public AnimationEventProcessor AnimationEventProcessorInstance;


        public static event Action OnSceneLoadFinishEvent;

        public static GlobalData Instance
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
                globalDataInstance = gameObject.GetComponent<GlobalData>();
            }

            // Hack, this should be in the project settings:
            Physics2D.queriesStartInColliders = false;
            AnimationEventProcessorInstance = new AnimationEventProcessor();

            // until we have loading and save data:
            ActiveSceneDynamicRef = StartSceneStaticRef;
            ActiveSceneDynamicRef.OnLoadInitialize();
        }

        void Start()
        {
            OnSceneLoadFinishEvent?.Invoke();
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
    }
}