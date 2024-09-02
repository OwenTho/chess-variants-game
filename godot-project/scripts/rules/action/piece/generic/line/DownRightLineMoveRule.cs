using Godot;

public partial class DownRightLineMoveRule : LineMoveRule
{
    internal override Vector2I[] GetDirs()
    {
        return new[] { GridVectors.DownRight };
    }
}