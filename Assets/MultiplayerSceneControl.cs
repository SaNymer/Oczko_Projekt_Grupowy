using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerSceneControl : MonoBehaviour
{
    public void StartServer()
    {
        SceneManager.LoadScene("ServerScene");
    }

    public void StartClient()
    {
        SceneManager.LoadScene("ClientScene");
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
