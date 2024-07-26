using System;
using Godot;

internal partial class PawnMoveAction : MoveAction
{
    public PawnMoveAction(Piece owner, Vector2I actionLocation, Vector2I moveLocation) : base(owner, actionLocation, moveLocation) { }

    public override void ActOn(GameState game, Piece piece)
    {
        base.ActOn(game, piece);
        piece.tags.Add("setup_pawn_initial");
    }

    public override object Clone()
    {
        PawnMoveAction newAction = new PawnMoveAction(null, actionLocation, moveLocation);
        CloneTo(newAction);
        return newAction;
    }
}