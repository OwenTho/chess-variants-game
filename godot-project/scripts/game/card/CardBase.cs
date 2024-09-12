using System;
using Godot;
using Godot.Collections;

[GlobalClass]
public abstract partial class CardBase : Node
{
    public const string CardIdKey = "card_id";
    public const string TeamIdKey = "team_id";
    
    public string cardId { get; internal set; }

    /// <summary>
    /// The team that owns this card. If it's -1, it's a Major Card and no team in
    /// specific owns it.
    /// </summary>
    public int teamId = -1;

    internal GameEvents cardNotices;

    public bool enabled = true;

    /// <summary>
    /// Whether the card is only processed on the server or not.
    /// </summary>
    public bool serverOnly { get; internal set; }

    /// <summary>
    /// If true, card does not have MakeListeners called and will be freed
    /// after OnAddCard is called.
    /// Immediate use Cards will break the game if they require any Waits.
    /// </summary>
    public bool immediateUse { get; internal set;}
    
    /// <summary>
    /// If true, the card will be added visually to the game.
    /// The placement is dependent on teamId.
    /// </summary>
    public bool displayCard { get; internal set; }

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

    private void EnsureBaseDictValue(Dictionary<string, string> cardDict, string id, string value)
    {
        if (cardDict.ContainsKey(id))
        {
            cardDict.Remove(id);
            GD.PushWarning($"Card {cardId} attempted to give dictionary with {id} set.");
        }
        cardDict.Add(id, value);
    }
    
    internal Dictionary<string, string> ConvertToDict(GameState game)
    {
        Dictionary<string, string> cardData = ToDict(game);
        EnsureBaseDictValue(cardData, CardIdKey, cardId);
        EnsureBaseDictValue(cardData, TeamIdKey, teamId.ToString());
        return cardData;
    }
    
    // Used for server sharing card information
    // card_id is automatically defined
    protected virtual Dictionary<string, string> ToDict(GameState game)
    {
        return new Dictionary<string, string>();
    }

    internal void ConvertFromDict(GameState game, Dictionary<string, string> dataDict)
    {
        // Allow card to process
        FromDict(game, dataDict);
        // But then process by itself
        if (dataDict.TryGetValue(TeamIdKey, out string teamIdValue))
        {
            if (!int.TryParse(teamIdValue, out teamId))
            {
                GD.PushError($"{TeamIdKey} should be an integer.");
            }
        }
        else
        {
            GD.PushError($"{TeamIdKey} not found in card data.");
        }
    }

    public virtual void FromDict(GameState game, Dictionary<string, string> dataDict)
    {
        
    }


    // By default, matching cards are ignored
    public virtual CardReturn OnMatchingCard(CardBase card)
    {
        return CardReturn.Ignored;
    }

    public void Wait(GameState game)
    {
        game.StartEventsWait();
    }

    public void EndWait(GameState game)
    {
        game.EndEventsWait();
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
