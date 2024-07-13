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
        InitRules();
        InitPieceInfo();
    }


    Registry<PieceInfo> pieceInfoRegistry = new Registry<PieceInfo>();
    Registry<RuleBase> ruleRegistry = new Registry<RuleBase>();

    public PieceInfo GetPieceInfo(string key)
    {
        return pieceInfoRegistry.GetValue(key);
    }

    public RuleBase GetRule(string key)
    {
        return ruleRegistry.GetValue(key);
    }

}
