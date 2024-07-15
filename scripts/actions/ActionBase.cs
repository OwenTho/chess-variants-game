using Godot;

public abstract partial class ActionBase : GodotObject
{
    public Vector2I actionLocation;

    public ActionBase(Vector2I actionLocation)
    {
        this.actionLocation = actionLocation;
    }

    public abstract void ActOn(Piece piece);
}
