using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProcessingHelpers;
using Library;
// this is persistent

namespace GNT
{
    public class GameManager : MonoBehaviour
    {
        [Header("Menual Setup")]
        private static GameManager globalDataInstance; // singleton

        public SceneInterface StartSceneStaticRef; // read only, set in the inspector only, invisible to other scripts
        public Camera MainCameraStaticRef; // read only, set in the inspector only, invisible to other scripts
        public PlayerController PlayerControllerStaticRef; // read only, reference needs to be set in the inspector, visible too other scripts
        public CharacterMovement PlayerMovementStaticRef; // read only, reference needs to be set in the inspector, visible too other scripts
        public CustomDialogueView DialogueViewStaticRef; // read only, reference needs to be set in the inspector, visible too other scripts
        public Yarn.Unity.DialogueRunner DialogueRunnerStaticRef; // read only, reference needs to be set in the inspector, visible too other scripts

        [Header("Global Data")]
        public float ForegroundShiftDuration;

        [Header("Debug View")]
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

            PlayerMovementStaticRef.LayerSwitchEvent += GameManager.Instance.OnLayerSwitch; // special handling when the player switches layers
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

        public void OnLayerSwitch(int oldIndex, int newIndex)
        {
            GroundLayer foreGroundLayer = ActiveSceneDynamicRef.GetGroundLayer(oldIndex);
            if (newIndex > oldIndex && foreGroundLayer.ShiftScale > 0.0f) // switched inwards (further away), the now foreground layer might need shifting down;
            {
                float downShiftScale = foreGroundLayer.ShiftScale;
                float distanceToScreenBottom = Helpers.GetDeltaYToScreenBottom(foreGroundLayer.transform.position, GetActiveCamera().transform.position, GetActiveCamera().fieldOfView);
                float boundingHeight = foreGroundLayer.GetBoundingHeight();

                // start shifing down
                ActiveSceneDynamicRef.ShiftForegroundDown(oldIndex, downShiftScale * (boundingHeight + distanceToScreenBottom), ForegroundShiftDuration);
            }
            else if (ActiveSceneDynamicRef.GetGroundLayer(newIndex).IsShiftedDown()) // switched out (closer)
            {
                // shift back up
                ActiveSceneDynamicRef.ShiftForegroundUp(newIndex, ForegroundShiftDuration);
            }
        }
    }
}