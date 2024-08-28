using System.Collections.Generic;
using Godot;

public partial class MoveOther : MoveAction
{
    private int movePieceId;
    public MoveOther(Piece owner, Vector2I actionLocation, Vector2I moveLocation, int movePieceId) : base(owner, actionLocation, moveLocation)
    {
        this.moveLocation = moveLocation;
        this.movePieceId = movePieceId;
    }

    public override void ActOn(GameState game, Piece piece)
    {
        // Move piece
        if (game.TryGetPiece(movePieceId, out Piece movePiece))
        {
            game.MovePiece(movePiece, moveLocation.X, moveLocation.Y);
            // Now that piece has moved, it needs to be updated
            movePiece.EnableActionsUpdate();
            movePiece.timesMoved += 1;
        }
    }

    public override object Clone()
    {
        MoveOther newMove = new MoveOther(null, actionLocation, moveLocation, -1);
        CloneTo(newMove);
        return newMove;
    }

    public override Dictionary<string, int> GetExtraCopyLinks()
    {
        Dictionary<string, int> newDictionary = new Dictionary<string, int>();
        if (attackAction != null)
        {
            newDictionary.Add("attackAction", attackAction.actionId);
        }

        if (movePieceId != null)
        {
            newDictionary.Add("movePiece", movePieceId);
        }

        return newDictionary;
    }

    public override void SetExtraCopyLinks(GameState game, Dictionary<string, int> extraLinks, Dictionary<int, ActionBase> links)
    {
        if (extraLinks.TryGetValue("attackAction", out int attackActionId))
        {
            if (links.TryGetValue(attackActionId, out ActionBase linkedAttackAction))
            {
                attackAction = (AttackAction)linkedAttackAction;
            }
        }
        
        if (extraLinks.TryGetValue("movePiece", out int pieceId))
        {
            movePieceId = pieceId;
        }
    }
}