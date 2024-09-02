using Godot;

public partial class DownLeftLineMoveRule : LineMoveRule
{
    internal override Vector2I[] GetDirs()
    {
        return new[] { GridVectors.DownLeft };
    }
}