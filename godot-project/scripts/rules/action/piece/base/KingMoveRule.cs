using Godot;
using Godot.Collections;


internal partial class KingMoveRule : ActionRuleBase
{
    public override void AddPossibleActions(GameController game, Piece piece)
    {
        Vector2I thisPosition = piece.cell.pos;
        Attack(piece, thisPosition + Vector2I.Down + Vector2I.Left, AttackType.IfMove); // Up Left
        Attack(piece, thisPosition + Vector2I.Down, AttackType.IfMove); // Up
        Attack(piece, thisPosition + Vector2I.Down + Vector2I.Right, AttackType.IfMove); // Up Right

        Attack(piece, thisPosition + Vector2I.Left, AttackType.IfMove); // Left
        Attack(piece, thisPosition + Vector2I.Right, AttackType.IfMove); // Right

        Attack(piece, thisPosition + Vector2I.Up + Vector2I.Left, AttackType.IfMove); // Down Left
        Attack(piece, thisPosition + Vector2I.Up, AttackType.IfMove); // Down
        Attack(piece, thisPosition + Vector2I.Up + Vector2I.Right, AttackType.IfMove); // Down Right
    }
}
