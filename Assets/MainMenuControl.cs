using Assets;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuControl : MonoBehaviour
{
    public InputField PlayerNameInput;
    public Button LoadGameButton;

    public void Awake()
    {
        SharedData.thisPlayer = new Player();
        PlayerNameInput.text = "Player";

        LoadGameButton.interactable = File.Exists("savedGame.txt");
    }

    public void StartSinglePlayerGame()
    {
        SharedData.thisPlayer.Name = PlayerNameInput.text;
        SharedData.thisPlayer.Id = 0;
        SceneManager.LoadScene("BlackJackGame");
    }

    public void StartMultiplayerGame()
    {
        SharedData.thisPlayer.Name = PlayerNameInput.text;
        SceneManager.LoadScene("MultiplayerScene");
    }

    public void ShowRanking()
    {
        SceneManager.LoadScene("RankingScene");
    }

    public void LoadGame()
    {
        using (var reader = new StreamReader("savedGame.txt"))
        {
            SharedData.thisPlayer.Name = reader.ReadLine();
            SharedData.thisPlayer.Score = int.Parse(reader.ReadLine());

            SceneManager.LoadScene("BlackJackGame");
        }
    }
}
