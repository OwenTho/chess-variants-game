using Godot;
using Godot.Collections;
using System.Collections.Generic;

public partial class AttackAction : ActionBase
{
    public Array<Piece> specificVictims; // Leave null unless needed
    public Vector2I attackLocation;
    public MoveAction moveAction;
    public AttackAction(Piece owner, Vector2I actionLocation, Vector2I attackLocation, MoveAction moveAction = null) : base(owner, actionLocation)
    {
        this.attackLocation = attackLocation;
        this.moveAction = moveAction;
    }

    public bool AddVictim(Piece piece)
    {
        if (specificVictims == null)
        {
            specificVictims = new Array<Piece>();
        }
        else if (specificVictims.Contains(piece))
        {
            return false;
        }
        specificVictims.Add(piece);
        return true;
    }

    public bool RemoveVictim(Piece piece)
    {
        if (specificVictims == null)
        {
            return false;
        }
        return specificVictims.Remove(piece);
    }

    public bool HasSpecificVictims()
    {
        if (specificVictims == null)
        {
            return false;
        }
        return specificVictims.Count > 0;
    }

    public override void ActOn(GameController game, Piece piece)
    {
        // If there are special victims, only take those
        if (HasSpecificVictims())
        {
            foreach (Piece victim in specificVictims)
            {
                game.TakePiece(victim);
            }
            return;
        }
        // Otherwise, if there are no victims, just take whatever isn't on this
        // piece's team.
        Array<Piece> targetedPieces = GetTargetedPieces(game);
        foreach (Piece victim in targetedPieces)
        {
            // Take the piece
            game.TakePiece(victim);
        }
    }

    public Array<Piece> GetTargetedPieces(GameController game)
    {
        return game.GetPiecesAt(attackLocation.X, attackLocation.Y);
    }

    public bool HasTargetedPieces(GameController game)
    {
        return game.HasPieceAt(attackLocation.X, attackLocation.Y);
    }
}