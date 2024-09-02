using Godot;

public partial class UpLineMoveRule : LineMoveRule
{
    internal override Vector2I[] GetDirs()
    {
        return new[] { GridVectors.Up };
    }
}