using System;
using Godot;
using Godot.Collections;

public abstract partial class CardBase : Node
{
    public string cardId { get; internal set; }

    public abstract void MakeListeners(GameEvents gameEvents);

    public abstract CardBase Clone();


    // Used for server sharing card information
    // card_id is automatically defined
    public virtual Dictionary<string, string> ToDict(GameState game)
    {
        return null;
    }

    public virtual void FromDict(GameState game, Dictionary<string, string> dataDict)
    {
        
    }
}
