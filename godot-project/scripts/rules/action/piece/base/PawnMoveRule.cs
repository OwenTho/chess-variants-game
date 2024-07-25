using Godot;
using Godot.Collections;
using System.Collections.Generic;

internal partial class PawnMoveRule : ActionRuleBase
{
    public override void AddPossibleActions(GameController game, Piece piece)
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
            piece.AddAction(newMove);
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
            piece.AddAction(newMove);
        }

        /// Attacking
        // Attacking is possible at diagonals
        MoveAction prevRight = null;
        MoveAction prevLeft = null;
        for (int i = 1; i <= maxForward; i++)
        {
            prevRight = Attack(piece, thisPosition + ((piece.forwardDirection + Vector2I.Right) * i), AttackType.MoveIf, prevRight).moveAction;
            prevLeft = Attack(piece, thisPosition + ((piece.forwardDirection + Vector2I.Left) * i), AttackType.MoveIf, prevLeft).moveAction;
        }

        // If next to a piece that moved twice, allow En passant
        CheckEnPassant(game, piece, thisPosition + Vector2I.Left);
        CheckEnPassant(game, piece, thisPosition + Vector2I.Right);
    }

    private void CheckEnPassant(GameController game, Piece piece, Vector2I position)
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
                    piece.AddAction(newAttack);

                    MoveAction newMove = new MoveAction(piece, attackPos, attackPos);
                    newMove.AddDependency(newAttack);
                    piece.AddAction(newMove);
                }
            }
        }
    }

    private bool CheckForPawn(GameController game, Vector2I position)
    {
        if (game.TryGetPiecesAt(position.X, position.Y, out Array<Piece> pieces))
        {
            foreach (Piece piece in pieces)
            {
                if (piece.info.pieceId == "pawn")
                {
                    return true;
                }
            }
        }

        return false;
    }

    public override void EndTurn(GameController game, Piece piece)
    {
        // If next to a pawn, EnPassant needs another check
        if (CheckForPawn(game, piece.cell.pos + Vector2I.Left) || CheckForPawn(game, piece.cell.pos + Vector2I.Right))
        {
            piece.EnableActionsUpdate();
        }
        piece.tags.Remove("pawn_initial");
        if (piece.tags.Contains("setup_pawn_initial"))
        {
            piece.tags.Remove("setup_pawn_initial");
            piece.tags.Add("pawn_initial");
        }
    }
}