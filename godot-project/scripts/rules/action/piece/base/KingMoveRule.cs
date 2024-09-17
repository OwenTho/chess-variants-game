using Godot;
using Godot.Collections;


internal partial class KingMoveRule : LineMoveRule
{
    public override void AddPossibleActions(GameState game, Piece piece, int level)
    {
        base.AddPossibleActions(game, piece, level);
        // In addition to the LineMove, also allow a single diagonal move
        // This results in the King being a slightly better Rook.
        Vector2I thisPosition = piece.cell.pos;
        Attack(piece, thisPosition + GridVectors.UpLeft, AttackType.IfMove); // Up Left
        Attack(piece, thisPosition + GridVectors.UpRight, AttackType.IfMove); // Up Right

        Attack(piece, thisPosition + GridVectors.DownLeft, AttackType.IfMove); // Down Left
        Attack(piece, thisPosition + GridVectors.DownRight, AttackType.IfMove); // Down Right
    }

    public override Vector2I[] GetDirs(GameState game, Piece piece)
    {
        return new[]
        {
            GridVectors.Up,
            GridVectors.Left,
            GridVectors.Right,
            GridVectors.Down
        };
    }
}
