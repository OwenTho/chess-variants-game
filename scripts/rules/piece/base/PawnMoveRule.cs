using Godot;
using System.Collections.Generic;

internal partial class PawnMoveRule : RuleBase
{
    public override List<ActionBase> AddPossibleActions(Piece piece, List<ActionBase> possibleActions)
    {
        int maxForward = piece.info.level;
        if (piece.timesMoved == 0)
        {
            maxForward += 1;
        }

        Vector2I thisPosition = new Vector2I(piece.cell.x, piece.cell.y);
        for (int i = 0; i < maxForward; i++)
        {
            possibleActions.Add(new MoveAction(thisPosition + (piece.forwardDirection * i)));
        }
        return possibleActions;
    }
}