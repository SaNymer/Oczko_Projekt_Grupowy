using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using System;

public class CardStack : MonoBehaviour
{
    List<int> cards;
    public bool HasCards { get { return cards != null && cards.Count > 0; } }
    public bool isGameDeck;
    public int CardCount { get { return cards == null ? 0 : cards.Count; } }
    public bool instaWin { get; private set; }

    public event CardEventHandler CardRemoved;
    public event CardEventHandler CardAdded;

    public void CreateDeck()
    {
        cards.Clear();

        for (int i = 0; i < 52; i++)
        {
            cards.Add(i);
        }

        var shuffledDeck = new List<int>();
        var rand = new System.Random();

        while (cards.Count > 0)
        {
            int index = rand.Next(0, cards.Count - 1);
            int cardNumber = cards[index];
            shuffledDeck.Add(cardNumber);
            cards.RemoveAt(index);
        }

        cards = shuffledDeck;
    }

	void Awake ()
    {
        cards = new List<int>();
        instaWin = false;
        if (isGameDeck)
        {
            CreateDeck();
        }
	}

    public int HandValue()
    {
        int total = 0;
        int aces = 0;

        foreach (int card in GetCards())
        {
            int cardRank = card % 13;

            if (cardRank == 0)          // Aces
            {
                cardRank += 11;
                aces++;
            }
            else if (cardRank <= 9)     // Normal numbers
                cardRank += 1;
            else if (cardRank == 10)    // Jack
                cardRank = 2;
            else if (cardRank == 11)    // Queen
                cardRank = 3;
            else                        // King
                cardRank = 4;

            total += cardRank;
        }

        if (total == 21 || aces == 2)
            instaWin = true;

        return total;
    }

    public IEnumerable<int> GetCards()
    {
        foreach (int i in cards)
        {
            yield return i;
        }
    }

    public int Pop()
    {
        int temp = cards[CardCount - 1];
        cards.RemoveAt(CardCount - 1);

        if (CardRemoved != null)
        {
            CardRemoved(this, new CardEventArgs(temp));
        }

        return temp;
    }

    public void Push(int card)
    {
        cards.Add(card);

        if (CardAdded != null)
        {
            CardAdded(this, new CardEventArgs(card));
        }
    }

    public void Reset()
    {
        cards.Clear();
    }
}
