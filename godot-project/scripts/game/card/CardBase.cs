using System;
using Godot;

public abstract partial class CardBase : Node
{
    public string cardId { get; internal set; }

    public abstract void MakeListeners(GameEvents gameEvents);

    public abstract CardBase Clone();
}
