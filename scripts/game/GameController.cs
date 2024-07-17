using Godot;
using System;

public partial class GameController : Node
{
    public Grid grid { get; private set; }

    public Vector2I gridSize = new Vector2I(8, 8);

    public Grid InitGrid()
    {
    }

    {
    }

    public PieceInfo GetPieceInfo(string key)
    {
        return pieceInfoRegistry.GetValue(key);
    }

    public RuleBase GetRule(string key)
    {
        return actionRuleRegistry.GetValue(key);
    }

}
