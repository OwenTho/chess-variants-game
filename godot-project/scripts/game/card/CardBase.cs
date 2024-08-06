using System;
using Godot;
using Godot.Collections;

public abstract partial class CardBase : Node
{
    public string cardId { get; internal set; }

    public abstract void MakeListeners(GameEvents gameEvents);

    public virtual void OnAddCard(GameState game)
    {
        
    }

    public abstract CardBase Clone();

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

    // Used for server sharing card information
    // card_id is automatically defined
    protected virtual Dictionary<string, string> ToDict(GameState game)
    {
        return new Dictionary<string, string>();
    }

    public virtual void FromDict(GameState game, Dictionary<string, string> dataDict)
    {
        
    }

    public virtual String GetName()
    {
        return StringUtil.ToTitleCase(cardId);
    }

    public virtual String GetImageLoc()
    {
        return "";
    }

    public virtual String GetDescription()
    {
        return "No description provided.";
    }
}
