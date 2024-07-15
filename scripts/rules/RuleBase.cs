using Godot;
using Godot.Collections;
using System.Collections.Generic;

public abstract partial class RuleBase : GodotObject
{

    public abstract Array<ActionBase> AddPossibleActions(Piece piece, Array<ActionBase> possibleActions);

    public Array<ActionBase> GetPossibleActions(Piece piece)
    {
        return AddPossibleActions(piece, new Array<ActionBase>());
    }

    public abstract void NewTurn(Piece piece);

    public string ruleId { get; internal set; }

}
