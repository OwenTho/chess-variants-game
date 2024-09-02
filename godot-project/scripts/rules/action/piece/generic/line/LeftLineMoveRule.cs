using Godot;

public partial class LeftLineMoveRule : LineMoveRule
{
    internal override Vector2I[] GetDirs()
    {
        return new[] { GridVectors.Left };
    }
}