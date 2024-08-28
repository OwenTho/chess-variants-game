﻿using Godot;
using Godot.Collections;

internal partial class PawnMoveRule : ActionRuleBase
{
    public override void AddPossibleActions(GameState game, Piece piece)
    {
        // Allow moving forward a number of spaces
        int maxForward = piece.info.level;
        Vector2I thisPosition = new Vector2I(piece.cell.x, piece.cell.y);
        MoveAction lastMove = null;
        for (int i = 1; i <= maxForward; i++)
        {
            Vector2I actionPos = thisPosition + (piece.forwardDirection.AsVector() * i);
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
            Vector2I actionPos = thisPosition + (piece.forwardDirection.AsVector() * (maxForward + 1));
            PawnMoveAction newMove = new PawnMoveAction(piece, actionPos, actionPos);
            if (lastMove != null)
            {
                newMove.AddDependency(lastMove);
            }
            // The action is unique, due to needing to allow En passant
            piece.AddAction(newMove);
        }

        
        // Attacking is possible at diagonals
        MoveAction prevRight = null;
        MoveAction prevLeft = null;
        for (int i = 1; i <= maxForward; i++)
        {
            prevRight = Attack(piece, thisPosition + ((piece.forwardDirection.AsVector() + Vector2I.Right) * i), AttackType.MoveIf, prevRight).moveAction;
            prevLeft = Attack(piece, thisPosition + ((piece.forwardDirection.AsVector() + Vector2I.Left) * i), AttackType.MoveIf, prevLeft).moveAction;
        }

        // If next to a piece that moved twice, allow En passant
        CheckEnPassant(game, piece, thisPosition + Vector2I.Left);
        CheckEnPassant(game, piece, thisPosition + Vector2I.Right);
    }

    private void CheckEnPassant(GameState game, Piece piece, Vector2I position)
    {
        if (game.TryGetPiecesAt(position.X, position.Y, out Array<Piece> pieces))
        {
            foreach (Piece victim in pieces)
            {
                // Ignore pieces of the same id, and that don't have the pawn_initial tag
                if (victim.teamId != piece.teamId && victim.tags.Contains("pawn_initial"))
                {
                    Vector2I attackPos = position + piece.forwardDirection.AsVector();
                    AttackAction newAttack = new AttackAction(piece, attackPos, attackPos);
                    newAttack.AddVictim(victim.id);
                    piece.AddAction(newAttack);

                    MoveAction newMove = new MoveAction(piece, attackPos, attackPos);
                    newMove.AddDependency(newAttack);
                    piece.AddAction(newMove);
                }
            }
        }
    }

    private bool CheckForPawn(GameState game, Vector2I position)
    {
        if (game.TryGetPiecesAt(position.X, position.Y, out Array<Piece> pieces))
        {
            foreach (Piece piece in pieces)
            {
                if (piece.info is { pieceId: "pawn" })
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool CheckNextToPawn(GameState game, Piece piece)
    {
        return CheckForPawn(game, piece.cell.pos + Vector2I.Left) ||
               CheckForPawn(game, piece.cell.pos + Vector2I.Right);
    }

    public override void EndTurn(GameState game, Piece piece)
    {
        // If next to a pawn, EnPassant needs another check
        if (piece.tags.Contains("by_pawn") || CheckNextToPawn(game, piece))
        {
            piece.tags.Add("by_pawn");
        }

        if (piece.tags.Contains("by_pawn"))
        {
            piece.EnableActionsUpdate();
            piece.tags.Remove("by_pawn");
        }
    }

    public override void NewTurn(GameState game, Piece piece)
    {
        // If next to a pawn, add the tag
        if (CheckNextToPawn(game, piece))
        {
            piece.tags.Add("by_pawn");
        }
        // If it's the piece's turn, remove pawn_initial
        if (piece.teamId == game.currentPlayerNum)
        {
            piece.tags.Remove("pawn_initial");
        }
    }
}