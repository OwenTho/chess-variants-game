using System;
using Godot;
using Godot.Collections;

public abstract partial class CardBase : Node
{
    public string cardId { get; internal set; }

    internal GameEvents cardNotices;

    public bool enabled = true;

    /// Whether the card is only processed on the server or not.
    public bool serverOnly { get; internal set; }

    [Signal]
    public delegate void CardInfoUpdatedEventHandler();
    
    public virtual void MakeListeners(GameEvents gameEvents)
    {
        
    }

    public void AddNotice(string noticeId, Action<GameState> listener)
    {
        cardNotices.AddListener(new EventListener(noticeId, listener));
    }

    public void AddListener(GameEvents gameEvents, string eventId, Action<GameState> listener, Func<GameState, EventResult> flagFunction = null)
    {
        gameEvents.AddListener(new EventListener(eventId, listener, flagFunction, CardIsEnabled));
    }

    public virtual void OnAddCard(GameState game)
    {
        
    }

    protected void SendCardNotice(GameState game, string notice)
    {
        game.SendCardNotice(this, notice);
    }

    public void ReceiveNotice(string noticeId)
    {
        cardNotices.AnnounceEvent(noticeId);
    }

    public bool CardIsEnabled()
    {
        return enabled;
    }

    public CardBase Clone()
    {
        CardBase newCard = CloneCard();
        newCard.cardId = cardId;
        newCard.enabled = enabled;
        return newCard;
    }
    
    protected abstract CardBase CloneCard();

    internal Dictionary<string, string> ConvertToDict(GameState game)
    {
        Dictionary<string, string> cardData = ToDict(game);
        if (cardData.ContainsKey("card_id"))
        {
            cardData.Remove("card_id");
            GD.PushWarning($"Card {cardId} attempted to give dictionary with card_id set.");
        }
        cardData.Add("card_id", cardId);
        return cardData;
    }

    // By default, matching cards are ignored
    public virtual CardReturn OnMatchingCard(CardBase card)
    {
        return CardReturn.Ignored;
    }

    // Used for server sharing card information
    // card_id is automatically defined
    protected virtual Dictionary<string, string> ToDict(GameState game)
    {
        return new Dictionary<string, string>();
    }

    public virtual void FromDict(GameState game, Dictionary<string, string> dataDict)
    {
        
    }

    public virtual String GetCardName()
    {
        return StringUtil.ToTitleCase(cardId);
    }

    public virtual String GetCardImageLoc()
    {
        return "";
    }

    public virtual String GetCardDescription()
    {
        return "No description provided.";
    }

    public void MakeNotices(GameState game)
    {
        cardNotices = new GameEvents(game);
    }
}
