using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public CardStack player;
    public CardStack opponent1;
    public CardStack opponent2;
    public CardStack opponent3;
    public CardStack deck;

    public Button hitButton;
    public Button stickButton;
    public Button playAgainButton;

    public Text winnerText;
    public Text playerHandValueText;
    public Text opponent1HandValueText;
    public Text opponent2HandValueText;
    public Text opponent3HandValueText;
    public Text scoreValue;

    private int score;

    public void Awake()
    {
        score = 0;
    }

    public void Exit()
    {
        using (var writer = new StreamWriter("scores.txt", true))
        {
            writer.WriteLine(SharedData.playerName + "\t" + score);
        }

        SceneManager.LoadScene("MainMenu");
    }

    public void Hit()
    {
        player.Push(deck.Pop());
        playerHandValueText.text = "Hand value: " + player.HandValue;
        if (player.HandValue > 21)
        {
            Stick();
        }
    }

    public void Stick()
    {
        hitButton.interactable = false;
        stickButton.interactable = false;
        // Dealer
        StartCoroutine(OpponentsTurn());
    }

    public void PlayAgain()
    {
        playAgainButton.interactable = false;
        hitButton.interactable = true;
        stickButton.interactable = true;
        winnerText.text = "";

        player.GetComponent<CardStackView>().Clear();
        opponent1.GetComponent<CardStackView>().Clear();
        opponent2.GetComponent<CardStackView>().Clear();
        opponent3.GetComponent<CardStackView>().Clear();
        deck.GetComponent<CardStackView>().Clear();

        deck.CreateDeck();
        StartGame();
    }

    void Start()
    {
        Screen.SetResolution(1366, 600, false);
        StartGame();
    }

    void StartGame()
    {
        for (int i = 0; i < 2; i++)
        {
            Hit();
            HitOpponent(opponent1);
            HitOpponent(opponent2);
            HitOpponent(opponent3);
        }

        playerHandValueText.text = "Hand value: " + player.HandValue;
        opponent1HandValueText.text = "";
        opponent2HandValueText.text = "";
        opponent3HandValueText.text = "";
        scoreValue.text = "Score: " + score;
    }

    void HitOpponent(CardStack opponent)
    {
        int card = deck.Pop();
        opponent.Push(card);
    }

    IEnumerator OpponentsTurn()
    {
        // AI
        yield return RunAI(opponent1);
        yield return RunAI(opponent2);
        yield return RunAI(opponent3);

        SetWinner();

        // Show all hidden cards
        ShowOpponentCards(opponent1);
        ShowOpponentCards(opponent2);
        ShowOpponentCards(opponent3);

        opponent1HandValueText.text = "Hand value: " + opponent1.HandValue;
        opponent2HandValueText.text = "Hand value: " + opponent2.HandValue;
        opponent3HandValueText.text = "Hand value: " + opponent3.HandValue;

        yield return new WaitForSeconds(0f);
        playAgainButton.interactable = true;
    }

    IEnumerator RunAI(CardStack opponent)
    {

        while (opponent.HandValue < 21)
        {
            var chanceToHit = (int)Math.Pow(21 - opponent.HandValue, 2)*2 - 2;
            var rand = new System.Random();

            if (chanceToHit < rand.Next(0, 100) || chanceToHit <= 0)
                break;

            HitOpponent(opponent);
            yield return new WaitForSeconds(0f);
        }
    }

    void ShowOpponentCards(CardStack opponent)
    {
        CardStackView view = opponent.GetComponent<CardStackView>();
        foreach (int card in opponent.GetCards())
        {
            view.Toggle(card, true);
        }
        view.ShowCards();
    }

    void SetWinner()
    {
        var handValues = new List<int>(){ opponent1.HandValue, opponent2.HandValue, opponent3.HandValue };
        var validHandValues = new List<int>() { 0 }; // We add 0 so the list won't be ever empty and Max method won't crash
        for (int i = 0; i < handValues.Count; i++)
        {
            if (handValues[i] <= 21)
                validHandValues.Add(handValues[i]);
        }
        var maxHandValue = validHandValues.Max();

        if ((player.WinStatus && (opponent1.WinStatus || opponent2.WinStatus || opponent3.WinStatus)) || player.HandValue == maxHandValue)
        {
            winnerText.text = "DRAW!";
        }
        else if (player.WinStatus || (player.HandValue > maxHandValue && player.HandValue <= 21) || (player.HandValue <= 21 && maxHandValue == 0))
        {
            winnerText.text = "You WON!";
            score++;
        }
        else
        {
            winnerText.text = "You LOST!";
            score--;
        }
    }
}
