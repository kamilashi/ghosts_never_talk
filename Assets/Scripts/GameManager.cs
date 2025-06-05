using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProcessingHelpers;
using Library;
using UnityEngine.SceneManagement;
// this is persistent

namespace GNT
{
    public class GameManager : MonoBehaviour
    {
        [Header("Menual Setup")]
        private static GameManager globalDataInstance; // singleton

        public SceneReference StartSceneReference;
        //public SceneInterface StartSceneStaticRef; 
        public Camera MainCameraStaticRef; 
        public CameraMovement CameraMovementStaticRef; 
        public PlayerController PlayerControllerStaticRef; 
        public CharacterMovement PlayerMovementStaticRef; 
        public CustomDialogueView DialogueViewStaticRef; 
        public Yarn.Unity.DialogueRunner DialogueRunnerStaticRef; 

        [Header("Global Data")]
        public float ForegroundShiftDuration;

        [Header("Debug View")]
        public SceneInterface ActiveSceneDynamicRef; //read + write only from the owner script #todo : maybe should be handled by the scene manager,global reference to which should be stored here - should be visible to other scripts

        public Dictionary<string, GlobalCharacterReference> GlobalCharacterRefDictionary;
        public AnimationEventProcessor AnimationEventProcessorInstance;

        public static event Action OnSceneLoadFinishEvent;

        public Scene LoadedLevelScene;
        //private List<AsyncOperation> loadOperations = new List<AsyncOperation>();
        private AsyncOperation loadOperation;
        private Coroutine sceneLoadingCoroutine;

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

            Debug.Assert(StartSceneReference.sceneName != "", "Please initialize the name of the scene " + this.name);

            LoadScene(StartSceneReference.sceneName);
        }

        void Update()
        {
            // #todo: move to some game processor/simulation script?
            AnimationEventProcessorInstance.Run(Time.deltaTime);
        }

        private void LoadScene(string sceneName)
        {
            loadOperation = (SceneManager.LoadSceneAsync(StartSceneReference.sceneName, LoadSceneMode.Additive));

            if (sceneLoadingCoroutine != null)
            {
                StopCoroutine(sceneLoadingCoroutine);
            }

            sceneLoadingCoroutine = StartCoroutine(LoadAdditiveScene(loadOperation, sceneName));
        }

        IEnumerator LoadAdditiveScene(AsyncOperation asyncLoad, string sceneName)
        {
            while (!asyncLoad.isDone)
                yield return null;

            Scene additiveScene = SceneManager.GetSceneByName(sceneName);
            while (!additiveScene.isLoaded)
                yield return null;

            LoadedLevelScene = additiveScene;
            GameObject sceneInterfaceGameObject = GameObject.FindWithTag("LevelScene");
            SceneInterface loadedScene = sceneInterfaceGameObject.GetComponent<SceneInterface>();

            StartOnLevel(loadedScene);
        }

        void StartOnLevel(SceneInterface levelSceneInterface)
        {
            OnSceneLoadFinishEvent?.Invoke();

            SetActiveScene(levelSceneInterface);

            ActiveSceneDynamicRef.OnLoadInitialize();

            // this will move into the scene init code
            {
                SceneStartData sceneStartData = ActiveSceneDynamicRef.GetSceneStartData();
                PlayerMovementStaticRef.SwitchToLayer(ActiveSceneDynamicRef.GetGroundLayer(sceneStartData.startLayerIdx), sceneStartData.positionOnLayer);

                Vector3 cameraPosition = PlayerMovementStaticRef.transform.position;
                cameraPosition.z += CameraMovementStaticRef.defaultPlayerOffsetZ;
                MainCameraStaticRef.transform.Translate(cameraPosition - MainCameraStaticRef.transform.position);
            }

            PlayerMovementStaticRef.LayerSwitchEvent += GameManager.Instance.OnLayerSwitch; // special handling when the player switches layers, should happen AFTER the first scene load layer switch
            PlayerControllerStaticRef.OnLevelLoadInitialize();
            CameraMovementStaticRef.OnLevelLoadInitialize();
        }

        private void SetActiveScene(SceneInterface sceneInterface)
        {
            ActiveSceneDynamicRef = sceneInterface;
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