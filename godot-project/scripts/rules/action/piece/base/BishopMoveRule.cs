using Godot;
using Godot.Collections;

internal partial class BishopMoveRule : LineMoveRule
{
    internal override Vector2I[] GetDirs()
    {
        return new Vector2I[] {
            Vector2I.Down + Vector2I.Right, // Up Right
            Vector2I.Down + Vector2I.Left, // Up Left
            Vector2I.Up + Vector2I.Right, // Down Right
            Vector2I.Up + Vector2I.Left // Down Left
        };
    }
}
