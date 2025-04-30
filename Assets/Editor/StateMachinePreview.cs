using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;

public class AnimatorPreviewTool : EditorWindow
{
    GameObject targetObject;
    Animator animator;
    AnimatorController controller;
    int selectedStateIndex;
    bool mirror;

    string[] stateNames;

    [MenuItem("Tools/Animator Preview Tool")]
    public static void ShowWindow()
    {
        GetWindow<AnimatorPreviewTool>("Animator Preview Tool");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Animator Preview Utility", EditorStyles.boldLabel);

        targetObject = (GameObject)EditorGUILayout.ObjectField("Target GameObject", targetObject, typeof(GameObject), true);

        if (targetObject != null)
        {
            animator = targetObject.GetComponent<Animator>();

            if (animator == null)
            {
                EditorGUILayout.HelpBox("Selected GameObject doesn't have an Animator.", MessageType.Warning);
                return;
            }

            controller = animator.runtimeAnimatorController as AnimatorController;

            if (controller == null)
            {
                EditorGUILayout.HelpBox("Animator doesn't use an AnimatorController.", MessageType.Warning);
                return;
            }

            if (stateNames == null || stateNames.Length == 0)
                LoadStates();

            selectedStateIndex = EditorGUILayout.Popup("Animation State", selectedStateIndex, stateNames);
            mirror = EditorGUILayout.Toggle("Is Mirrored", mirror);

            if (GUILayout.Button("Play Selected State"))
            {
                PlayStateInEditor();
            }
        }
    }

    void LoadStates()
    {
        var states = controller.layers[0].stateMachine.states;
        stateNames = new string[states.Length];
        for (int i = 0; i < states.Length; i++)
        {
            stateNames[i] = states[i].state.name;
        }
    }

    void PlayStateInEditor()
    {
        if (animator == null || controller == null) return;

        string stateName = stateNames[selectedStateIndex];

#if UNITY_EDITOR
        AnimatorControllerLayer layer = controller.layers[0];
        AnimatorState state = layer.stateMachine.states[selectedStateIndex].state;

        AnimationMode.StartAnimationMode();
        AnimationMode.SampleAnimationClip(targetObject, state.motion as AnimationClip, 0f);
        animator.Rebind();
        animator.Update(0f);

        //animator.SetBool("Mirror", mirror); // assumes you have an "isMirrored" param
#endif
    }

    private void OnDisable()
    {
        AnimationMode.StopAnimationMode();
    }
}
