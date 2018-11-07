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

    public event CardRemovedEventHandler CardRemoved;

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

	void Start ()
    {
        cards = new List<int>();
        if (isGameDeck)
        {
            CreateDeck();
        }
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
            CardRemoved(this, new CardRemovedEventArgs(temp));
        }

        return temp;
    }

    public void Push(int card)
    {
        cards.Add(card);
    }
}
