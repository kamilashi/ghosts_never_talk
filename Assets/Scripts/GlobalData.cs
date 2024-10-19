using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this is persistent

namespace GNT
{
    public class GlobalData : MonoBehaviour
    {
        private static GlobalData globalDataInstance;

        [SerializeField]
        private SceneInterface startScene; // set i the inspector only

        public static event Action OnSceneLoadFinishEvent;

        public SceneInterface activeScene;

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
                globalDataInstance = gameObject.GetComponentInChildren<GlobalData>();
            }

            // Hack, this should be in the project settings:
            Physics2D.queriesStartInColliders = false;
        }

        void Start()
        {
            // until we have loading and save data:
            activeScene = startScene;
            OnSceneLoadFinishEvent?.Invoke();
        }

        void Update()
        {

        }
    }
}