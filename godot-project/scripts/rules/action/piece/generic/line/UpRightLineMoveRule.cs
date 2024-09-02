using Godot;

public partial class UpRightLineMoveRule : LineMoveRule
{
    internal override Vector2I[] GetDirs()
    {
        return new[] { GridVectors.UpRight };
    }
}