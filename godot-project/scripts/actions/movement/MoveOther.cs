using System.Collections.Generic;
using Godot;

public partial class MoveOther : MoveAction
{
    private Piece movePiece;
    public MoveOther(Piece owner, Vector2I actionLocation, Vector2I moveLocation, Piece movePiece) : base(owner, actionLocation, moveLocation)
    {
        this.moveLocation = moveLocation;
        this.movePiece = movePiece;
    }

    public override void ActOn(GameState game, Piece piece)
    {
        // Move piece
        game.MovePiece(movePiece, moveLocation.X, moveLocation.Y);
        // Now that piece has moved, it needs to be updated
        movePiece.EnableActionsUpdate();
        movePiece.timesMoved += 1;
    }

    public override object Clone()
    {
        MoveOther newMove = new MoveOther(null, actionLocation, moveLocation, null);
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

        if (movePiece != null)
        {
            newDictionary.Add("movePiece", movePiece.id);
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
            if (game.TryGetPiece(pieceId, out Piece piece))
            {
                movePiece = piece;
            }
        }
    }
}