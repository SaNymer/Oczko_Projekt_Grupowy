using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuControl : MonoBehaviour
{
    public void StartSinglePlayerGame()
    {
        SceneManager.LoadScene("BlackJackGame");
    }
}
