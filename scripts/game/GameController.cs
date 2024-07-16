using Godot;
using System;

public partial class GameController : Node
{
    public Grid grid { get; private set; }

    public Grid InitGrid()
    {
        grid = new Grid();
        return grid;
    }

    public void FullInit()
    {
        InitGrid();
        InitValidationRules();
        InitActionRules();
        InitPieceInfo();
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
