using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal partial class PawnMoveAction : MoveAction
{
    public PawnMoveAction(Vector2I actionLocation, Vector2I moveLocation) : base(actionLocation, moveLocation) { }

    public override void ActOn(Piece piece)
    {
        base.ActOn(piece);
        piece.tags.Add("pawn_initial");
    }
}