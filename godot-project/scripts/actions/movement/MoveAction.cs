using Godot;
using System.Collections.Generic;

public partial class MoveAction : ActionBase
{
    public Vector2I moveLocation;
    public AttackAction attackAction;
    public MoveAction(Piece owner, Vector2I actionLocation, Vector2I moveLocation) : base(owner, actionLocation)
    {
        this.moveLocation = moveLocation;
    }

    public override void ActOn(GameController game, Piece piece)
    {
        // Move piece
        game.grid.PlaceItemAt(piece, actionLocation.X, actionLocation.Y);
        // Now that piece has moved, it needs to be updated
        piece.EnableActionsUpdate();
        piece.timesMoved += 1;
    }
}