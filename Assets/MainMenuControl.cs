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
        SharedData.playerScore = new PlayerScore();
        PlayerNameInput.text = "Player";

        LoadGameButton.interactable = File.Exists("savedGame.txt");
    }

    public void StartSinglePlayerGame()
    {
        SharedData.playerScore.Name = PlayerNameInput.text;
        SceneManager.LoadScene("BlackJackGame");
    }

    public void ShowRanking()
    {
        SceneManager.LoadScene("RankingScene");
    }

    public void LoadGame()
    {
        using (var reader = new StreamReader("savedGame.txt"))
        {
            SharedData.playerScore.Name = reader.ReadLine();
            SharedData.playerScore.Score = int.Parse(reader.ReadLine());

            SceneManager.LoadScene("BlackJackGame");
        }
    }
}
