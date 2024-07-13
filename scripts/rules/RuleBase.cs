using Godot;
using System.Collections.Generic;

public abstract partial class RuleBase : GodotObject
{

    public abstract List<Vector2I> GetPossibleMoves(Piece piece);
    public abstract List<Vector2I> GetPossibleAttacks(Piece piece);

    public string ruleId { get; internal set; }

}
