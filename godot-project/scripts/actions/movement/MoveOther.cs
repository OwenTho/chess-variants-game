using System.Collections.Generic;
using Godot;

public partial class MoveOther : MoveAction
{
    private int movePieceId;

    public MoveOther() : base()
    {
        
    }
    
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

    protected override ActionBase Clone()
    {
        MoveOther newMove = new MoveOther(null, actionLocation, moveLocation, -1);
        return newMove;
    }

    public override Godot.Collections.Dictionary<string, string> ToDict()
    {
        // Get the base dictionary from MoveAction
        Godot.Collections.Dictionary<string, string> actionDict = base.ToDict();
        
        actionDict.Add("move_piece_id", movePieceId.ToString());
        return actionDict;
    }

    public override void FromDict(Godot.Collections.Dictionary<string, string> actionDict)
    {
        base.FromDict(actionDict);
        if (actionDict.TryGetValue("move_piece_id", out string dictMovePieceId))
        {
            if (int.TryParse(dictMovePieceId, out int newMovePieceId))
            {
                movePieceId = newMovePieceId;
            }
            else
            {
                GD.PushError("move_piece_id was not a number.");
            }
        }
        else
        {
            GD.PushError("move_piece_id not found in dictionary.");
        }
    }
}