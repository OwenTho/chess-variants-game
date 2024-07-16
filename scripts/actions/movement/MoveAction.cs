using Godot;
using System.Collections.Generic;

public partial class MoveAction : ActionBase
{
    public Vector2I moveLocation;
    public MoveAction(Vector2I actionLocation, Vector2I moveLocation) : base(actionLocation)
    {
        this.moveLocation = moveLocation;
    }

    public override void ActOn(Piece piece)
    {
        piece.cell.grid.PlaceItemAt(piece, actionLocation.X, actionLocation.Y);
        piece.timesMoved += 1;
    }
}