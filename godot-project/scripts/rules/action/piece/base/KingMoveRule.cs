using Godot;
using Godot.Collections;


internal partial class KingMoveRule : ActionRuleBase
{
    public override void AddPossibleActions(GameState game, Piece piece, int level)
    {
        Vector2I thisPosition = piece.cell.pos;
        Attack(piece, thisPosition + GridVectors.UpLeft, AttackType.IfMove); // Up Left
        Attack(piece, thisPosition + GridVectors.Up, AttackType.IfMove); // Up
        Attack(piece, thisPosition + GridVectors.UpRight, AttackType.IfMove); // Up Right

        Attack(piece, thisPosition + GridVectors.Left, AttackType.IfMove); // Left
        Attack(piece, thisPosition + GridVectors.Right, AttackType.IfMove); // Right

        Attack(piece, thisPosition + GridVectors.DownLeft, AttackType.IfMove); // Down Left
        Attack(piece, thisPosition + GridVectors.Down, AttackType.IfMove); // Down
        Attack(piece, thisPosition + GridVectors.DownRight, AttackType.IfMove); // Down Right
    }
}
