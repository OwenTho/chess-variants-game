using System.Collections.Generic;
using Godot;

public partial class CardDeck : Node
{
    private class CardInfo
    {
        public CardFactory Factory;
        public int CardsLeft;

        public CardInfo(CardFactory factory, int cardsLeft)
        {
            Factory = factory;
            CardsLeft = cardsLeft;
        }
    }

    private Dictionary<string, CardInfo> _cards = new();
    private RandomNumberGenerator _random = new();
    public bool WeightCards = false;

    public void AddCard(string cardId, int count = 1)
    {
        if (!_cards.TryGetValue(cardId, out CardInfo cardInfo))
        {
            GD.PushError($"Tried to add {count} cards of id {cardId}, but the factory has not been added first.");
            return;
        }

        cardInfo.CardsLeft += count;
    }
    
    public void AddCard(CardFactory cardFactory, int count = 1)
    {
        string cardId = cardFactory.cardId;
        if (_cards.TryGetValue(cardId, out CardInfo cardInfo))
        {
            // If the factories don't match, error
            if (cardInfo.Factory != cardFactory)
            {
                GD.PushError($"Id {cardId} is already registered to a different card factory.");
                return;
            }
            
            // Add the count
            cardInfo.CardsLeft += count;

            return;
        }
        
        _cards.Add(cardFactory.cardId, new CardInfo(cardFactory, count));
    }

    public void RemoveCard(string cardId, int count = 1)
    {
        AddCard(cardId, -count);
    }

    public void RemoveCard(CardFactory cardFactory, int count = 1)
    {
        AddCard(cardFactory, -count);
    }


    // Put card back into the pile, claiming it unused.
    public bool PutCard(CardBase card)
    {
        // Check if the card id exists
        if (!_cards.TryGetValue(card.cardId, out CardInfo cardInfo))
        {
            // If it doesn't, error
            GD.PushError($"Tried to put card {card.cardId} in the deck, but its factory isn't present.");
            return false;
        }
        
        // Tell the factory to remove the card
        if (!cardInfo.Factory.ReturnCard(card))
        {
            // If it's not made by this factory, error
            GD.PushError($"Tried to return a card of id {card.cardId} to a deck, but the CardFactory didn't create it.");
            return false;
        }
        // If it succeeded in putting the card away, add one card to the deck
        cardInfo.CardsLeft += 1;
        return true;
    }

    public CardBase PullCard(GameState game)
    {
        // Randomly pull a card from the Deck (only for cards with 1 or more)
        List<CardInfo> validCards = new List<CardInfo>();
        int totalWeight = 0;
        foreach (var card in _cards.Values)
        {
            if (card.CardsLeft > 0 && card.Factory.CanMakeNewCard(game))
            {
                validCards.Add(card);
                totalWeight += card.CardsLeft;
            }
        }
        
        // If there are no cards, return null
        if (validCards.Count == 0)
        {
            return null;
        }
        
        // Randomly select a Card from the Deck
        int selectedCard;
        if (WeightCards)
        {
            // If weighted, randomly select from 0 - weight total, and pick the matching card
            selectedCard = _random.RandiRange(0, totalWeight);
            for (var i = 0 ; i < validCards.Count ; i++)
            {
                CardInfo card = validCards[i];
                totalWeight -= card.CardsLeft;
                if (totalWeight <= 0)
                {
                    selectedCard = i;
                }
            }
        }
        else
        {
            selectedCard = _random.RandiRange(0, validCards.Count - 1);
        }
        
        // Remove 1 instance of the card, and 
        CardInfo cardInfo = validCards[selectedCard];
        cardInfo.CardsLeft -= 1;
        return cardInfo.Factory.CreateNewCard(game);
    }
}
