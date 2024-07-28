using Godot;

namespace ChessVariantsGame.scripts.actions.movement;

public partial class SlideAction : MoveAction
{
    public SlideAction(Piece owner, Vector2I actionLocation, Vector2I moveLocation) : base(owner, actionLocation, moveLocation)
    {
        acting = false;
    }
    
    public override void ActOn(GameState game, Piece piece)
    {
        // No actual movement, as it's a "mid-movement" so dependents can be cancelled.
    }
}