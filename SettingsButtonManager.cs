using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsButtonManager : MonoBehaviour
{
    private void Start() 
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void QuitGame() 
    {
        Application.Quit();
    }

    public void TestScene() 
    {
        SceneManager.LoadScene(sceneName: "TestScene");
    }

    public void Tutorial()
    {
        SceneManager.LoadScene(sceneName: "Tutorial");
    }
}
