using Godot;
using Godot.Collections;
using System.Collections.Generic;

internal partial class PawnMoveRule : ActionRuleBase
{
    public override Array<ActionBase> AddPossibleActions(GameController game, Piece piece, Array<ActionBase> possibleActions)
    {
        /// Movement
        // Allow moving forward a number of spaces
        int maxForward = piece.info.level;
        Vector2I thisPosition = new Vector2I(piece.cell.x, piece.cell.y);
        MoveAction lastMove = null;
        for (int i = 1; i <= maxForward; i++)
        {
            Vector2I actionPos = thisPosition + (piece.forwardDirection * i);
            MoveAction newMove = new MoveAction(piece, actionPos, actionPos);
            if (lastMove != null)
            {
                newMove.AddDependency(lastMove);
            }
            possibleActions.Add(newMove);
            lastMove = newMove;
        }

        // Allow an extra space forward for the first turn
        if (piece.timesMoved == 0)
        {
            Vector2I actionPos = thisPosition + (piece.forwardDirection * (maxForward + 1));
            PawnMoveAction newMove = new PawnMoveAction(piece, actionPos, actionPos);
            if (lastMove != null)
            {
                newMove.AddDependency(lastMove);
            }
            // The action is unique, due to needing to allow En passant
            possibleActions.Add(newMove);
        }

        /// Attacking
        // Attacking is possible at diagonals
        for (int i = 1; i <= maxForward; i++)
        {
            // TODO: Does not currently add dependencies to the previous attack (so pawn can jump over pieces at higher levels)
            Attack(piece.grid, piece, thisPosition + ((piece.forwardDirection + Vector2I.Right) * i), possibleActions, AttackType.MoveIf);
            Attack(piece.grid, piece, thisPosition + ((piece.forwardDirection + Vector2I.Left) * i), possibleActions, AttackType.MoveIf);
        }

        // If next to a piece that moved twice, allow En passant
        CheckEnPassant(game, piece, thisPosition + Vector2I.Left, possibleActions);
        CheckEnPassant(game, piece, thisPosition + Vector2I.Right, possibleActions);
        return possibleActions;
    }

    private void CheckEnPassant(GameController game, Piece piece, Vector2I position, Array<ActionBase> possibleActions)
    {
        if (game.TryGetPiecesAt(position.X, position.Y, out Array<Piece> pieces))
        {
            foreach (Piece victim in pieces)
            {
                if (victim.tags.Contains("pawn_initial"))
                {
                    Vector2I attackPos = position + piece.forwardDirection;
                    AttackAction newAttack = new AttackAction(piece, attackPos, attackPos);
                    newAttack.AddVictim(victim);
                    possibleActions.Add(newAttack);

                    MoveAction newMove = new MoveAction(piece, attackPos, attackPos);
                    newMove.AddDependency(newAttack);
                    possibleActions.Add(newMove);
                }
            }
        }
    }

    public override void NewTurn(GameController game, Piece piece)
    {
        piece.tags.Remove("pawn_initial");
        if (piece.tags.Contains("setup_pawn_initial"))
        {
            piece.tags.Remove("setup_pawn_initial");
            piece.tags.Add("pawn_initial");
        }
    }
}