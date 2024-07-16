using Godot;
using System.Collections.Generic;

public abstract partial class ActionBase : GodotObject
{
    public Vector2I actionLocation;
    internal bool valid = true;

    // Rules that this rule depends on. If these are invalid, this one should
    // also be invalid.
    internal List<ActionBase> dependsOn = new List<ActionBase>();

    public ActionBase(Vector2I actionLocation)
    {
        this.actionLocation = actionLocation;
    }

    public abstract void ActOn(Piece piece);

    public void DependsOn(ActionBase action)
    {
        if (action == null)
        {
            return;
        }
        dependsOn.Add(action);
    }
}
