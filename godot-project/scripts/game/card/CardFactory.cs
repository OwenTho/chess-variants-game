using System.Collections.Generic;
using Godot;

public abstract partial class CardFactory : Node {
  public string CardId { get; internal set; }
  public bool ServerOnly { get; internal set; }
  public bool ImmediateUse { get; internal set; }
  public bool DisplayCard { get; internal set; }

  private readonly List<CardBase> _createdCards = new();

  public CardBase CreateNewBlankCard(GameState game) {
    CardBase newCard = CreateBlankCard(game);
    newCard.CardId = CardId;
    newCard.ServerOnly = ServerOnly;
    newCard.ImmediateUse = ImmediateUse;
    newCard.DisplayCard = DisplayCard;
    _createdCards.Add(newCard);
    return newCard;
  }

  // Returns the cards that currently exist with the cardId that are in the game.
  public List<CardBase> GetExistingCards(GameState game) {
    return game.GetExistingCards(CardId);
  }

  /// <summary>
  /// Checks and returns whether a card can be created or not given the current state of the game.
  /// </summary>
  /// <param name="game">The current GameState</param>
  /// <returns>bool - Whether a new card can be created or not.</returns>
  public abstract bool CanMakeNewCard(GameState game);


  internal CardBase CreateNewCard(GameState game) {
    CardBase newCard = CreateCard(game);
    newCard.CardId = CardId;
    newCard.ServerOnly = ServerOnly;
    newCard.ImmediateUse = ImmediateUse;
    newCard.DisplayCard = DisplayCard;
    _createdCards.Add(newCard);
    return newCard;
  }

  internal CardBase CreateNewCardFromDict(GameState game,
    Godot.Collections.Dictionary<string, string> cardData) {
    CardBase newCard = CreateFromDict(game, cardData);
    newCard.CardId = CardId;
    newCard.ServerOnly = ServerOnly;
    newCard.ImmediateUse = ImmediateUse;
    newCard.DisplayCard = DisplayCard;
    _createdCards.Add(newCard);
    return newCard;
  }

  internal bool ReturnCard(CardBase card) {
    return _createdCards.Remove(card);
  }


  // Create the card from a dictionary. By default, this calls the blank card function and
  // then calls FromDict().
  protected virtual CardBase CreateFromDict(GameState game,
    Godot.Collections.Dictionary<string, string> cardData) {
    CardBase newCard = CreateBlankCard(game);
    newCard.ConvertFromDict(game, cardData);
    return newCard;
  }

  // Function for initial creation of the card. This is separate from the CreateNewCard, as
  // it forces the cardId to be the same as the factory's.
  protected abstract CardBase CreateCard(GameState game);

  // A blank instance of the Card, so that any cards that process the board normally don't need to do so.
  protected abstract CardBase CreateBlankCard(GameState game);
}