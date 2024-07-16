using Godot;
using Godot.Collections;


internal partial class KingMoveRule : ActionRuleBase
{
    public override Array<ActionBase> AddPossibleActions(Piece piece, Array<ActionBase> possibleActions)
    {
        Vector2I thisPosition = piece.cell.pos;
        Attack(piece.grid, thisPosition + Vector2I.Down + Vector2I.Left, possibleActions, AttackType.AlsoMove); // Up Left
        Attack(piece.grid, thisPosition + Vector2I.Down, possibleActions, AttackType.AlsoMove); // Up
        Attack(piece.grid, thisPosition + Vector2I.Down + Vector2I.Right, possibleActions, AttackType.AlsoMove); // Up Right

        Attack(piece.grid, thisPosition + Vector2I.Left, possibleActions, AttackType.AlsoMove); // Left
        Attack(piece.grid, thisPosition + Vector2I.Right, possibleActions, AttackType.AlsoMove); // Right

        Attack(piece.grid, thisPosition + Vector2I.Up + Vector2I.Left, possibleActions, AttackType.AlsoMove); // Down Left
        Attack(piece.grid, thisPosition + Vector2I.Up, possibleActions, AttackType.AlsoMove); // Down
        Attack(piece.grid, thisPosition + Vector2I.Up + Vector2I.Right, possibleActions, AttackType.AlsoMove); // Down Right
        return possibleActions;
    }

    public override void NewTurn(Piece piece)
    {
        throw new System.NotImplementedException();
    }
}
