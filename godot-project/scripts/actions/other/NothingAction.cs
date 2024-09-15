
using Godot;
using Godot.Collections;

public partial class NothingAction : ActionBase
{
    public NothingAction() : base()
    {
        
    }
    
    public NothingAction(Piece owner, Vector2I actionLocation) : base(owner, actionLocation)
    {
        
    }

    public override void ActOn(GameState game, Piece piece)
    {
        
    }

    protected override ActionBase Clone()
    {
        return new NothingAction(owner, actionLocation);
    }

    public override void FromDict(Dictionary<string, string> actionDict)
    {
        
    }
}