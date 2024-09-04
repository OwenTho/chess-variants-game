using Godot;
using Godot.Collections;

public partial class MoveAction : ActionBase
{
    public Vector2I moveLocation;
    // Only on server
    public AttackAction attackAction;

    public MoveAction() : base()
    {
        
    }
    
    public MoveAction(Piece owner, Vector2I actionLocation, Vector2I moveLocation) : base(owner, actionLocation)
    {
        this.moveLocation = moveLocation;
    }

    public override void ActOn(GameState game, Piece piece)
    {
        // Announce the event, and stop if cancelled / not continuing
        if (!game.gameEvents.AnnounceEvent(GameEvents.MovePiece))
        {
            return;
        }
        game.lastMovePiece = piece;
        // Move piece
        game.MovePiece(piece, moveLocation.X, moveLocation.Y);
        game.gameEvents.AnnounceEvent(GameEvents.PieceMoved);
        // Now that piece has moved, it needs to be updated
        piece.EnableActionsUpdate();
        piece.timesMoved += 1;
    }

    public override object Clone()
    {
        MoveAction newMove = new MoveAction(null, actionLocation, moveLocation);
        CloneTo(newMove);
        return newMove;
    }

    public override Dictionary<string, string> ToDict()
    {
        Dictionary<string, string> actionDict = new();
        AddVector2IToDict("move_loc", moveLocation, actionDict);
        return actionDict;
    }

    public override void FromDict(Dictionary<string, string> actionDict)
    {
        moveLocation = ReadVector2IFromDict("move_loc", actionDict);
    }
}