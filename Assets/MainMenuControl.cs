using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuControl : MonoBehaviour
{
    public InputField PlayerNameInput;

    public void Awake()
    {
        PlayerNameInput.text = "Player";
    }

    public void StartSinglePlayerGame()
    {
        SharedData.playerName = PlayerNameInput.text;
        SceneManager.LoadScene("BlackJackGame");
    }
}
