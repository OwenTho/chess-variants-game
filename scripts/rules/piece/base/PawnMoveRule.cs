using Godot;
using System.Collections.Generic;

internal partial class PawnMoveRule : RuleBase
{
    public override List<Vector2I> GetPossibleMoves(Piece piece)
    {
        int maxForward = piece.info.level;
        if (piece.timesMoved == 0)
        {
            maxForward += 1;
        }

        List<Vector2I> possibleMoves = new List<Vector2I>();
        Vector2I thisPosition = new Vector2I(piece.cell.x, piece.cell.y);
        for (int i = 0; i < maxForward; i++)
        {
            possibleMoves.Add(thisPosition + (piece.forwardDirection * i));
        }
        return possibleMoves;
    }

    public override List<Vector2I> GetPossibleAttacks(Piece piece)
    {
        throw new System.NotImplementedException();
    }
}