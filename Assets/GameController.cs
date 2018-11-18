using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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


    // Cards dealt to each player
    // First player hits/sticks/bust
    // Dealer's turn; must have minimum 17 score hand
    // Dealer cards; first card is hidden, subsequent cards are facing

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
    }

    void HitOpponent(CardStack opponent)
    {
        int card = deck.Pop();
        opponent.Push(card);
    }

    IEnumerator OpponentsTurn()
    {
        // AI
        //while (opponent1.HandValue < 17)
        //{
        //    HitOpponent(opponent1);
        //    yield return new WaitForSeconds(1f);
        //}

        //while (opponent2.HandValue < 17)
        //{
        //    HitOpponent(opponent2);
        //    yield return new WaitForSeconds(1f);
        //}

        //while (opponent3.HandValue < 17)
        //{
        //    HitOpponent(opponent3);
        //    yield return new WaitForSeconds(1f);
        //}
        yield return RunAI(opponent1);
        yield return RunAI(opponent2);
        yield return RunAI(opponent3);

        // Set winner
        if (player.HandValue > 21 || (opponent2.HandValue >= player.HandValue && opponent2.HandValue <= 21)) // Dealer wins
        {
            winnerText.text = "You LOST!";
        }
        else if (opponent2.HandValue > 21 || (player.HandValue >= opponent2.HandValue && player.HandValue <= 21)) // Player wins
        {
            winnerText.text = "You WON!";
        }
        else
        {
            // Draw
            winnerText.text = "DRAW!";
        }

        // Show all hidden cards
        ShowOpponentCards(opponent1);
        ShowOpponentCards(opponent2);
        ShowOpponentCards(opponent3);

        opponent1HandValueText.text = "Hand value: " + opponent1.HandValue;
        opponent2HandValueText.text = "Hand value: " + opponent2.HandValue;
        opponent3HandValueText.text = "Hand value: " + opponent3.HandValue;

        yield return new WaitForSeconds(1f);
        playAgainButton.interactable = true;
    }

    IEnumerator RunAI(CardStack opponent)
    {
        while (opponent.HandValue < 17)
        {
            HitOpponent(opponent);
            yield return new WaitForSeconds(1f);
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
}
