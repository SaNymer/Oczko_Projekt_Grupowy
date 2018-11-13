using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public CardStack player;
    public CardStack dealer;
    public CardStack deck;

    public Button hitButton;
    public Button stickButton;
    public Button playAgainButton;

    public Text winnerText;
    public Text handValueText;

    // Cards dealt to each player
    // First player hits/sticks/bust
    // Dealer's turn; must have minimum 17 score hand
    // Dealer cards; first card is hidden, subsequent cards are facing
	
    public void Hit()
    {
        player.Push(deck.Pop());
        handValueText.text = "Hand value: " + player.HandValue();
        if (player.HandValue() > 21)
        {
            hitButton.interactable = false;
            stickButton.interactable = false;

            StartCoroutine(DealersTurn());
        }
    }

    public void Stick()
    {
        hitButton.interactable = false;
        stickButton.interactable = false;
        // Dealer
        StartCoroutine(DealersTurn());
    }

    public void PlayAgain()
    {
        playAgainButton.interactable = false;
        hitButton.interactable = true;
        stickButton.interactable = true;
        winnerText.text = "";

        player.GetComponent<CardStackView>().Clear();
        dealer.GetComponent<CardStackView>().Clear();
        deck.GetComponent<CardStackView>().Clear();

        deck.CreateDeck();
        StartGame();
    }

    void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        for (int i = 0; i < 2; i++)
        {
            Hit();
            HitDealer();
        }
    }

    void HitDealer()
    {
        int card = deck.Pop();
        dealer.Push(card);

        // While opponent turn we can show hit cards
        //if (dealer.CardCount >= 2)
        //{
        //    CardStackView view = dealer.GetComponent<CardStackView>();
        //    view.Toggle(card, true);
        //}
    }

    IEnumerator DealersTurn()
    {
        hitButton.interactable = false;
        stickButton.interactable = false;

        // AI
        while (dealer.HandValue() < 17)
        {
            HitDealer();
            yield return new WaitForSeconds(1f);
        }

        // Set winner
        if (!player.instaWin && ( player.HandValue() > 21 || (dealer.HandValue() >= player.HandValue() && dealer.HandValue() <= 21))) // Dealer wins
        {
            winnerText.text = "You LOST!";
        }
        else if (!dealer.instaWin && (dealer.HandValue() > 21 || (player.HandValue() >= dealer.HandValue() && player.HandValue() <= 21))) // Player wins
        {
            winnerText.text = "You WON!";
        }
        else
        {
            // Draw
            winnerText.text = "DRAW!";
        }

        // Show all hidden cards

        yield return new WaitForSeconds(1f);
        playAgainButton.interactable = true;
    }
}
