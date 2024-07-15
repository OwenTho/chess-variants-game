using Godot;
using Godot.Collections;
using System.Collections.Generic;

internal partial class PawnMoveRule : RuleBase
{
    public override Array<ActionBase> AddPossibleActions(Piece piece, Array<ActionBase> possibleActions)
    {
        /// Movement
        // Allow moving forward a number of spaces
        int maxForward = piece.info.level;
        Vector2I thisPosition = new Vector2I(piece.cell.x, piece.cell.y);
        for (int i = 1; i <= maxForward; i++)
        {
            possibleActions.Add(new MoveAction(thisPosition + (piece.forwardDirection * i)));
        }

        // Allow an extra space forward for the first turn
        if (piece.timesMoved == 0)
        {
            // The action is unique, due to needing to allow En passant
            possibleActions.Add(new PawnMoveAction(thisPosition + (piece.forwardDirection * (maxForward + 1))));
        }

        /// Attacking
        // Attacking is possible at diagonals
        for (int i = 1; i <= maxForward; i++)
        {
            // possibleActions.Add(new AttackAction(thisPosition + (piece.forwardDirection * i) + Vector2I.Right));
            // possibleActions.Add(new AttackAction(thisPosition + (piece.forwardDirection * i) + Vector2I.Left));
        }

        // If next to a piece that moved twice, allow En passant
        CheckEnPassant(piece, thisPosition + Vector2I.Left, possibleActions);
        CheckEnPassant(piece, thisPosition + Vector2I.Right, possibleActions);
        return possibleActions;
    }

    private void CheckEnPassant(Piece piece, Vector2I position, Array<ActionBase> possibleActions)
    {
        if (piece.cell.grid.TryGetCellAt(position.X, position.Y, out GridCell cell))
        {
            // If there is a cell, check for a piece
            foreach (GridItem item in cell.items)
            {
                if (item is Piece)
                {
                    Piece thisPiece = (Piece)item;
                    if (thisPiece.tags.Contains("pawn_initial"))
                    {
                        possibleActions.Add(new AttackAction(cell.pos, thisPiece));
                    }
                }
            }
        }
    }

    public override void NewTurn(Piece piece)
    {
        piece.tags.Remove("pawn_initial");
    }
}