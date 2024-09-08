
using System.Collections.Generic;
using Godot;


public abstract partial class CardFactory : Node
{
    public string cardId { get; internal set; }
    public bool serverOnly { get; internal set; }
    public bool immediateUse { get; internal set; }
    
    private List<CardBase> _createdCards = new();
    internal CardBase CreateNewCard(GameState game)
    {
        CardBase newCard = CreateCard(game);
        newCard.cardId = cardId;
        newCard.serverOnly = serverOnly;
        newCard.immediateUse = immediateUse;
        _createdCards.Add(newCard);
        return newCard;
    }
    internal CardBase CreateNewCardFromDict(GameState game, Godot.Collections.Dictionary<string, string> cardData)
    {
        CardBase newCard = CreateFromDict(game, cardData);
        newCard.cardId = cardId;
        newCard.serverOnly = serverOnly;
        newCard.immediateUse = immediateUse;
        _createdCards.Add(newCard);
        return newCard;
    }

    internal bool ReturnCard(CardBase card)
    {
        return _createdCards.Remove(card);
    }
    
    // Function for initial creation of the card. This is separate from the CreateNewCard, as
    // it forces the cardId to be the same as the factory's.
    protected abstract CardBase CreateCard(GameState game);


    public CardBase CreateNewBlankCard(GameState game)
    {
        CardBase newCard = CreateBlankCard(game);
        newCard.cardId = cardId;
        newCard.serverOnly = serverOnly;
        newCard.immediateUse = immediateUse;
        _createdCards.Add(newCard);
        return newCard;
    }

    // A blank instance of the Card, so that any cards that process the board normally don't need to do so.
    protected abstract CardBase CreateBlankCard(GameState game);

    // Create the card from a dictionary. By default, this calls the blank card function and
    // then calls FromDict().
    protected virtual CardBase CreateFromDict(GameState game, Godot.Collections.Dictionary<string, string> cardData)
    {
        CardBase newCard = CreateBlankCard(game);
        newCard.FromDict(game, cardData);
        newCard.cardId = cardId;
        newCard.serverOnly = serverOnly;
        newCard.immediateUse = immediateUse;
        _createdCards.Add(newCard);
        return newCard;
    }

    // Function for if the card can be created on the state of the game.
    // For example, a card that changes one piece into another already on the board can't be used if
    // all pieces are the same.
    public abstract bool CanMakeNewCard(GameState game);

    // Returns the cards that currently exist with the cardId that are in the game.
    public List<CardBase> GetExistingCards(GameState game)
    {
        return game.GetExistingCards(cardId);
    }
}
