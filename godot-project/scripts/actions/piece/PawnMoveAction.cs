using System;
using Godot;

internal partial class PawnMoveAction : MoveAction
{
    public PawnMoveAction() : base()
    {
        
    }
    
    public PawnMoveAction(Piece owner, Vector2I actionLocation, Vector2I moveLocation) : base(owner, actionLocation, moveLocation) { }

    public override void ActOn(GameState game, Piece piece)
    {
        base.ActOn(game, piece);
        piece.tags.Add("pawn_initial");
    }

    protected override ActionBase Clone()
    {
        PawnMoveAction newAction = new PawnMoveAction(null, actionLocation, moveLocation);
        return newAction;
    }
}