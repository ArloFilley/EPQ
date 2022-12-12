using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{

    [Header("Keybinds")]
    public KeyCode exitKey = KeyCode.Escape;

    void Update()
    {
        if (Input.GetKey(exitKey)) {
            SceneManager.LoadScene(sceneName: "SettingsScene");
        }
    }
}
