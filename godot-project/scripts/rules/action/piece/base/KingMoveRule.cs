using Godot;
using Godot.Collections;


internal partial class KingMoveRule : ActionRuleBase
{
    public override Array<ActionBase> AddPossibleActions(GameController game, Piece piece, Array<ActionBase> possibleActions)
    {
        Vector2I thisPosition = piece.cell.pos;
        Attack(piece.grid, piece, thisPosition + Vector2I.Down + Vector2I.Left, possibleActions, AttackType.IfMove); // Up Left
        Attack(piece.grid, piece, thisPosition + Vector2I.Down, possibleActions, AttackType.IfMove); // Up
        Attack(piece.grid, piece, thisPosition + Vector2I.Down + Vector2I.Right, possibleActions, AttackType.IfMove); // Up Right

        Attack(piece.grid, piece, thisPosition + Vector2I.Left, possibleActions, AttackType.IfMove); // Left
        Attack(piece.grid, piece, thisPosition + Vector2I.Right, possibleActions, AttackType.IfMove); // Right

        Attack(piece.grid, piece, thisPosition + Vector2I.Up + Vector2I.Left, possibleActions, AttackType.IfMove); // Down Left
        Attack(piece.grid, piece, thisPosition + Vector2I.Up, possibleActions, AttackType.IfMove); // Down
        Attack(piece.grid, piece, thisPosition + Vector2I.Up + Vector2I.Right, possibleActions, AttackType.IfMove); // Down Right
        return possibleActions;
    }
}
