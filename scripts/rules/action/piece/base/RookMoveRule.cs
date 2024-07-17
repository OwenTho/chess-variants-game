﻿using Godot;
using Godot.Collections;

internal partial class RookMoveRule : LineMoveRule
{
    internal override Vector2I[] GetDirs()
    {
        return new Vector2I[] {
            Vector2I.Down, // Up
            Vector2I.Up, // Down
            Vector2I.Left, // Right
            Vector2I.Right // Left
        };
    }
}
