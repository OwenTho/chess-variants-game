using Godot;
using System.Collections.Generic;
using Godot.Collections;

public partial class AttackAction : ActionBase
{
    public int specificVictimId = -1; // Leave null unless needed
    public Vector2I attackLocation;
    public MoveAction moveAction;
    public AttackAction(Piece owner, Vector2I actionLocation, Vector2I attackLocation, MoveAction moveAction = null) : base(owner, actionLocation)
    {
        this.attackLocation = attackLocation;
        this.moveAction = moveAction;
    }

    public void AddVictim(int pieceId)
    {
        specificVictimId = pieceId;
    }

    public bool HasSpecificVictims()
    {
        if (specificVictimId <= -1)
        {
            return false;
        }

        return true;
    }

    public override void ActOn(GameState game, Piece piece)
    {
        // Announce the event, and stop if cancelled / not continuing
        if (!game.gameEvents.AnnounceEvent(GameEvents.AttackPiece))
        {
            return;
        }
        // If there are special victims, only take those
        if (HasSpecificVictims())
        {
            game.TakePiece(specificVictimId, piece.id);
            return;
        }
        // Otherwise, if there are no victims, just take whatever isn't on this
        // piece's team.
        Array<Piece> targetedPieces = GetTargetedPieces(game);
        foreach (Piece victim in targetedPieces)
        {
            // Take the piece
            game.TakePiece(victim, piece);
        }
    }

    public Array<Piece> GetTargetedPieces(GameState game)
    {
        return game.GetPiecesAt(attackLocation.X, attackLocation.Y);
    }

    public bool HasTargetedPieces(GameState game)
    {
        return game.HasPieceAt(attackLocation.X, attackLocation.Y);
    }

    public override object Clone()
    {
        AttackAction newAttack = new AttackAction(null, actionLocation, attackLocation, null);
        CloneTo(newAttack);
        return newAttack;
    }

    public override System.Collections.Generic.Dictionary<string, int> GetExtraCopyLinks()
    {
        System.Collections.Generic.Dictionary<string, int> newDictionary = new();
        if (moveAction != null)
        {
            newDictionary.Add("moveAction", moveAction.actionId);
        }

        if (specificVictimId != null)
        {
            newDictionary.Add("victim", specificVictimId);
        }

        return newDictionary;
    }

    public override void SetExtraCopyLinks(GameState game, System.Collections.Generic.Dictionary<string, int> extraLinks, System.Collections.Generic.Dictionary<int, ActionBase> links)
    {
        if (extraLinks.TryGetValue("moveAction", out int attackActionId))
        {
            if (links.TryGetValue(attackActionId, out ActionBase linkedMoveAction))
            {
                moveAction = (MoveAction)linkedMoveAction;
            }
        }

        if (extraLinks.TryGetValue("victim", out int id))
        {
            specificVictimId = id;
        }
    }
}