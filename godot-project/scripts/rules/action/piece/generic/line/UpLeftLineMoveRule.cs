using Godot;

public partial class UpLeftLineMoveRule : LineMoveRule
{
    internal override Vector2I[] GetDirs()
    {
        return new[] { GridVectors.UpLeft };
    }
}