using Godot;
using System.Collections.Generic;

public abstract partial class ActionBase : GodotObject
{
    public Vector2I actionLocation;
    internal bool valid = true;

    internal List<ActionBase> dependentActions = new List<ActionBase>();

    public ActionBase(Vector2I actionLocation)
    {
        this.actionLocation = actionLocation;
    }

    public abstract void ActOn(Piece piece);
}
