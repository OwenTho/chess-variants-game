using Godot;

internal partial class PawnMoveAction : MoveAction
{
    public PawnMoveAction(Piece owner, Vector2I actionLocation, Vector2I moveLocation) : base(owner, actionLocation, moveLocation) { }

    public override void ActOn(GameController game, Piece piece)
    {
        base.ActOn(game, piece);
        piece.tags.Add("setup_pawn_initial");
    }
}