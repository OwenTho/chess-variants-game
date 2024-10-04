using System;
using Godot;
using Godot.Collections;

[GlobalClass]
public abstract partial class CardBase : Node {
  [Signal]
  public delegate void CardInfoUpdatedEventHandler();

  public const string CardIdKey = "card_id";
  public const string TeamIdKey = "team_id";

  public const string PieceNameColour = "aqua";
  public const string EmphasisColour = "red";
  public const string WarningColour = "salmon";

  public bool Enabled = true;

  /// <summary>
  ///   The team that owns this card. If it's -1, it's a Major Card and no team in
  ///   specific owns it.
  /// </summary>
  public int TeamId = -1;

  public string CardId { get; internal set; }

  /// <summary>
  ///   Whether the card is only processed on the server or not.
  /// </summary>
  public bool ServerOnly { get; internal set; }

  /// <summary>
  ///   If true, card does not have MakeListeners called and will be freed
  ///   after OnAddCard is called.
  ///   Immediate use Cards will break the game if they require any Waits.
  /// </summary>
  public bool ImmediateUse { get; internal set; }

  /// <summary>
  ///   If true, the card will be added visually to the game.
  ///   The placement is dependent on teamId.
  /// </summary>
  public bool DisplayCard { get; internal set; }

  internal GameEvents CardNotices;


  public void MakeNotices(GameState game) {
    CardNotices = new GameEvents(game);
  }

  public virtual void MakeListeners(GameEvents gameEvents) {
  }

  public void AddNotice(string noticeId, Action<GameState> listener) {
    CardNotices.AddListener(new EventListener(noticeId, listener));
  }

  public void AddListener(GameEvents gameEvents, string eventId, Action<GameState> listener,
    Func<GameState, EventResult> flagFunction = null) {
    gameEvents.AddListener(new EventListener(eventId, listener, flagFunction, CardIsEnabled));
  }

  public virtual void OnAddCard(GameState game) {
  }

  public void ReceiveNotice(string noticeId) {
    CardNotices.AnnounceEvent(noticeId);
  }

  public bool CardIsEnabled() {
    return Enabled;
  }

  public CardBase Clone() {
    CardBase newCard = CloneCard();
    newCard.CardId = CardId;
    newCard.Enabled = Enabled;
    return newCard;
  }


  public void Wait(GameState game) {
    game.StartEventsWait();
  }

  public void EndWait(GameState game) {
    game.EndEventsWait();
  }


  // By default, matching cards are ignored
  public virtual CardReturn OnMatchingCard(CardBase card) {
    return CardReturn.Ignored;
  }

  public virtual string GetCardName() {
    return StringUtil.ToTitleCase(CardId);
  }

  public virtual string GetCardName(GameState game) {
    return GetCardName();
  }

  public virtual string GetCardImageLoc() {
    return "missing.png";
  }

  public virtual string GetCardImageLoc(GameState game) {
    return GetCardImageLoc();
  }

  public virtual string GetCardDescription() {
    return "No description provided.";
  }

  public virtual string GetCardDescription(GameState game) {
    return GetCardDescription();
  }


  public virtual void FromDict(GameState game, Dictionary<string, string> dataDict) {
  }


  internal Dictionary<string, string> ConvertToDict(GameState game) {
    Dictionary<string, string> cardData = ToDict(game);
    EnsureBaseDictValue(cardData, CardIdKey, CardId);
    EnsureBaseDictValue(cardData, TeamIdKey, TeamId.ToString());
    return cardData;
  }

  internal void ConvertFromDict(GameState game, Dictionary<string, string> dataDict) {
    // Allow card to process
    FromDict(game, dataDict);
    // But then process by itself
    if (dataDict.TryGetValue(TeamIdKey, out string teamIdValue)) {
      if (!int.TryParse(teamIdValue, out TeamId)) {
        GD.PushError($"{TeamIdKey} should be an integer.");
      }
    }
    else {
      GD.PushError($"{TeamIdKey} not found in card data.");
    }
  }

  protected void SendCardNotice(GameState game, string notice) {
    game.SendCardNotice(this, notice);
  }

  // Used for server sharing card information
  // card_id is automatically defined
  protected virtual Dictionary<string, string> ToDict(GameState game) {
    return new Dictionary<string, string>();
  }

  protected abstract CardBase CloneCard();


  private void EnsureBaseDictValue(Dictionary<string, string> cardDict, string id, string value) {
    if (cardDict.ContainsKey(id)) {
      cardDict.Remove(id);
      GD.PushWarning($"Card {CardId} attempted to give dictionary with {id} set.");
    }

    cardDict.Add(id, value);
  }
}