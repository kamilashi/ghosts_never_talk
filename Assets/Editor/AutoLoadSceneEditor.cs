using GNT;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

static class AutoLoadSceneEditor
{
    private static string persistentScenePath = "Assets/Scenes/PersistentScene.unity";

    [InitializeOnLoadMethod]
    static void OnLoad()
    {
        Debug.Log("[Probe] InitializeOnLoadMethod fired.");

        EditorApplication.playModeStateChanged += (PlayModeStateChange c) =>
        {
            Debug.Log($"[Probe] playModeStateChanged: {c}");
            if(c == PlayModeStateChange.EnteredPlayMode)
            {
                SceneManager.LoadScene("PersistentScene");
            }
        };
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        // Check if the opened scene is not the one we want
        if (scene.path != persistentScenePath)
        {
            // Delay the loading to avoid conflicts
            EditorApplication.delayCall += () =>
            {
                EditorSceneManager.OpenScene(persistentScenePath, OpenSceneMode.Single);
            };
        }

        Debug.Log($"[Probe] sceneOpened: {scene.path} ({mode})");
    }
}