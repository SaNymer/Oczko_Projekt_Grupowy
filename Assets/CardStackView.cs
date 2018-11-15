using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CardStack))]
public class CardStackView : MonoBehaviour
{
    CardStack deck;
    Dictionary<int, CardView> fetchedCards;
    int lastCount;

    public Vector3 start;
    public GameObject cardPrefab;
    public float cardOffset;
    public bool faceUp = false;
    public bool isHorizontal = true;

    public void Toggle(int card, bool isFaceUp)
    {
        fetchedCards[card].IsFaceUp = isFaceUp;
    }

	void Awake()
    {
        fetchedCards = new Dictionary<int, CardView>();
        deck = GetComponent<CardStack>();
        ShowCards();
        lastCount = deck.CardCount;

        deck.CardRemoved += Deck_CardRemoved;
        deck.CardAdded += Deck_CardAdded;
    }

    private void Deck_CardAdded(object sender, CardEventArgs e)
    {
        AddCard(CalculateNextCardPosition(deck.CardCount), e.CardIndex, deck.CardCount);
    }

    private void Deck_CardRemoved(object sender, CardEventArgs e)
    {
        if (fetchedCards.ContainsKey(e.CardIndex))
        {
            Destroy(fetchedCards[e.CardIndex].Card);
            fetchedCards.Remove(e.CardIndex);
        }
    }

    void Update()
    {
        if (lastCount != deck.CardCount)
        {
            lastCount = deck.CardCount;
            ShowCards();
        }
    }

    public void ShowCards()
    {
        int cardCount = 0;

        if (!deck.HasCards)
            return;

        foreach (int i in deck.GetCards())
        {
            AddCard(CalculateNextCardPosition(cardCount), i, cardCount);

            cardCount++;
        }
    }

    void AddCard(Vector3 position, int cardIndex, int positionalIndex)
    {
        if (fetchedCards.ContainsKey(cardIndex))
        {
            if (!faceUp)
            {
                CardModel model = fetchedCards[cardIndex].Card.GetComponent<CardModel>();
                model.ToggleFace(fetchedCards[cardIndex].IsFaceUp);
            }

            return;
        }

        GameObject cardCopy = (GameObject)Instantiate(cardPrefab);
        cardCopy.transform.position = position;

        CardModel cardModel = cardCopy.GetComponent<CardModel>();
        cardModel.cardIndex = cardIndex;
        cardModel.ToggleFace(faceUp);

        SpriteRenderer spriteRenderer = cardCopy.GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = positionalIndex;
        if (!isHorizontal)
            spriteRenderer.transform.Rotate(0, 0, 90);

        fetchedCards.Add(cardIndex, new CardView(cardCopy));
    }

    public void Clear()
    {
        deck.Reset();

        foreach (CardView view in fetchedCards.Values)
        {
            Destroy(view.Card);
        }

        fetchedCards.Clear();
    }

    Vector3 CalculateNextCardPosition(int cardCount)
    {
        float co = cardOffset * cardCount;

        Vector3 newPos = start;
        if (isHorizontal)
            newPos += new Vector3(co, 0f);
        else
            newPos += new Vector3(0f, -co);

        return newPos;
    }
}
