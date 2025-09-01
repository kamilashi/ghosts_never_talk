using GNT;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuEntry : MonoBehaviour
{
    public SceneReference SceneReference;
    public MainMenu MainMenuStaticRef;

    //private TMP_Text buttonText;

    // Start is called before the first frame update
    void Awake()
    {
        Debug.Assert(SceneReference.sceneName != "", "Please initialize the name of the scene on " + this.name);

        //buttonText = GetComponentInChildren<TMP_Text>();
        //buttonText.text = SceneReference.sceneName;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnLevelSelectButtonClicked()
    {
        MainMenuStaticRef.OnLoadLevelPressed(SceneReference.sceneName);
    }
}
