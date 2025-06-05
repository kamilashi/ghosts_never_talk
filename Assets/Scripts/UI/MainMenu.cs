using GNT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private Canvas mainMenuCanvas;

    void Awake()
    {
        mainMenuCanvas = gameObject.GetComponent<Canvas>();
    }
    void Start()
    {
        GameManager.OnSceneLoadFinishEvent += ExitMenu;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnExitButtonPressed()
    {
        Application.Quit();
    }
    
    public void OnLoadLevelPressed(string levelSceneName)
    {
        GameManager.Instance.LoadScene(levelSceneName);
    }

    public void ToggleMenu()
    {
        if (mainMenuCanvas.enabled)
        {
            ExitMenu();
        }
        else
        {
            EnterMenu();
        }
    }

    private void ExitMenu()
    {
        mainMenuCanvas.enabled = false;
    }
    private void EnterMenu()
    {
        mainMenuCanvas.enabled = true;
    }
}
