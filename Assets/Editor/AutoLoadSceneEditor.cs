using GNT;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

static class AutoLoadSceneEditor
{
    //private static string persistentScenePath = "Assets/Scenes/PersistentScene.unity";
    private static string persistentSceneName = "PersistentScene";

    [InitializeOnLoadMethod]
    static void OnLoad()
    {
        Debug.Log("[Probe] InitializeOnLoadMethod fired.");

        EditorApplication.playModeStateChanged += (PlayModeStateChange c) =>
        {
            if(c == PlayModeStateChange.EnteredPlayMode && SceneManager.GetActiveScene().name != persistentSceneName)
            {
                SceneManager.LoadScene(persistentSceneName);
            }
        };
    }
}