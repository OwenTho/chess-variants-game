using Godot;
using Godot.Collections;

internal partial class QueenMoveRule : LineMoveRule
{

    internal override Vector2I[] GetDirs()
    {
        return new Vector2I[] {
            Vector2I.Down, // Up
            Vector2I.Down + Vector2I.Right, // Up Right
            Vector2I.Down + Vector2I.Left, // Up Left
            Vector2I.Left, // Left
            Vector2I.Right, // Right
            Vector2I.Up + Vector2I.Right, // Down Right
            Vector2I.Up, // Down
            Vector2I.Up + Vector2I.Left // Down Left
        };
    }

    public override void NewTurn(Piece piece)
    {
        return;
    }
}
