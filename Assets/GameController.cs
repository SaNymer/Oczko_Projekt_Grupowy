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

    // Cards dealt to each player
    // First player hits/sticks/bust
    // Dealer's turn; must have minimum 17 score hand
    // Dealer cards; first card is hidden, subsequent cards are facing
	
    public void Hit()
    {
        player.Push(deck.Pop());
        if (player.HandValue() > 21)
        {
            // player busted
            hitButton.interactable = false;
            stickButton.interactable = false;
        }
    }

    public void Stick()
    {
        hitButton.interactable = false;
        stickButton.interactable = false;
        // Dealer
        StartCoroutine(DealersTurn());
    }

    void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        for (int i = 0; i < 2; i++)
        {
            player.Push(deck.Pop());
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
        while (dealer.HandValue() < 17)
        {
            HitDealer();
            yield return new WaitForSeconds(5f);
        }
    }
}
