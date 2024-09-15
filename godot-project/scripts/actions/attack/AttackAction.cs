using Godot;
using Godot.Collections;

public partial class AttackAction : ActionBase
{
    public int specificVictimId = -1; // Leave null unless needed
    public Vector2I attackLocation;
    // Only on server
    public MoveAction moveAction;

    public AttackAction() : base()
    {
        
    }
    
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

    protected override ActionBase Clone()
    {
        AttackAction newAttack = new AttackAction(null, actionLocation, attackLocation, null);
        return newAttack;
    }

    public override Dictionary<string, string> ToDict()
    {
        Dictionary<string, string> actionDict = new();
        AddVector2IToDict("attack_loc", attackLocation, actionDict);
        if (HasSpecificVictims())
        {
            actionDict.Add("victim_id", specificVictimId.ToString());
        }
        return actionDict;
    }

    public override void FromDict(Dictionary<string, string> actionDict)
    {
        attackLocation = ReadVector2IFromDict("attack_loc", actionDict);
        if (actionDict.TryGetValue("victim_id", out string dictVictimId))
        {
            if (int.TryParse(dictVictimId, out int victimId))
            {
                specificVictimId = victimId;
            }
            else
            {
                GD.PushError("victim_id is not an int.");
            }
        }
        else
        {
            specificVictimId = -1;
        }
    }
}