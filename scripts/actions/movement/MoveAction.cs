using Godot;
using System.Collections.Generic;

public partial class MoveAction : ActionBase
{
    public MoveAction(Vector2I moveLocation) : base(moveLocation)
    {

    }

    public override void ActOn(Piece piece)
    {
        piece.cell.grid.PlaceItemAt(piece, actionLocation.X, actionLocation.Y);
    }
}