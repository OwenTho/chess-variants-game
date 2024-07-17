using Godot;

public partial class AttackAction : ActionBase
{
    public Piece victim;
    public MoveAction moveAction;
    public AttackAction(Vector2I actionLocation, Piece victim, MoveAction moveAction = null) : base(actionLocation)
    {
        this.victim = victim;
        this.moveAction = moveAction;
    }

    public override void ActOn(Piece piece)
    {
        // Attack the piece
        piece.cell.grid.RemoveItem(victim);
    }
}