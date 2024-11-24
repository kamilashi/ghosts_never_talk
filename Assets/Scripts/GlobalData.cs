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

        [SerializeField]
        private SceneInterface startScene; // read only, set in the inspector only, invisible to other scripts
        [SerializeField]
        private Camera mainCamera; // read only, set in the inspector only, invisible to other scripts
        [SerializeField] // store reference to game object instead?
        private PlayerController playerController; // read only, reference needs to be set in the inspector, visible too other scripts


        public SceneInterface ActiveScene; //read + write only from the owner script #todo : maybe should be handled by the scene manager,global reference to which should be stored here - should be visible to other scripts

        public AnimationEventProcessor animationEventProcessor;


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
            animationEventProcessor = new AnimationEventProcessor();

            // until we have loading and save data:
            ActiveScene = startScene;
            ActiveScene.OnLoadInitialize();
        }

        void Start()
        {
            OnSceneLoadFinishEvent?.Invoke();
        }

        void Update()
        {
            // #todo: move to some game processor/simulation script?
            animationEventProcessor.Run(Time.deltaTime);
        }

        public Camera GetActiveCamera()
        {
            return mainCamera;
        }

        public PlayerController GetPlayerController()
        {
            return playerController;
        }
    }
}