using Godot;
using Godot.Collections;

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

    public override void ActOn(GameState game, Piece piece)
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
        AttackAction newAttack = new AttackAction(null, actionLocation, attackLocation, moveAction);
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

        if (specificVictims != null)
        {
            for (int i = 0; i < specificVictims.Count; i++)
            {
                newDictionary.Add($"victim_{i}", specificVictims[i].id);
            }
        }

        return newDictionary;
    }

    public override void SetExtraCopyLinks(GameState game, System.Collections.Generic.Dictionary<string, int> extraLinks, System.Collections.Generic.Dictionary<int, ActionBase> links)
    {
        if (extraLinks.TryGetValue("attackAction", out int attackActionId))
        {
            if (links.TryGetValue(attackActionId, out ActionBase moveAction))
            {
                this.moveAction = (MoveAction)moveAction;
            }
        }

        int i = 0;
        while (extraLinks.TryGetValue($"victim_{i}", out int id))
        {
            if (game.TryGetPiece(id, out Piece piece))
            {
                specificVictims.Add(piece);
            }
            i++;
        }
    }
}