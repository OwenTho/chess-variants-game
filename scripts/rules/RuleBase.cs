using Godot;
using System.Collections.Generic;

public abstract partial class RuleBase : GodotObject
{

    public abstract List<ActionBase> AddPossibleActions(Piece piece, List<ActionBase> possibleActions);

    public List<ActionBase> GetPossibleActions(Piece piece)
    {
        return AddPossibleActions(piece, new List<ActionBase>());
    }

    public abstract void NewTurn(Piece piece);

    public string ruleId { get; internal set; }

}
