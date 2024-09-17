using Godot;

public partial class SlideAction : MoveAction
{
    public SlideAction(Piece owner, Vector2I slideLocation) : base(owner, slideLocation, slideLocation)
    {
        acting = false;
    }
    
    public override void ActOn(GameState game, Piece piece)
    {
        // No actual movement, as it's a "mid-movement" so dependents can be cancelled.
    }
}