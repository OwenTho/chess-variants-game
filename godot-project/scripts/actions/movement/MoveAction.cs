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
        game.grid.PlaceItemAt(piece, actionLocation.X, actionLocation.Y);
        piece.timesMoved += 1;
    }
}