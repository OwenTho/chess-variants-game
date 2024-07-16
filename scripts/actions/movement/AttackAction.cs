using Godot;

public partial class AttackAction : ActionBase
{
    public Piece victim;
    public AttackAction(Vector2I actionLocation, Piece victim) : base(actionLocation)
    {
        this.victim = victim;
    }

    public override void ActOn(Piece piece)
    {
        // Attack the piece
        piece.cell.grid.RemoveItem(victim);
    }
}