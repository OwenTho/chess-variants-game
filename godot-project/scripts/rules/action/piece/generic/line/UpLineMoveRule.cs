using Godot;

public partial class UpLineMoveRule : LineMoveRule
{
    public override Vector2I[] GetDirs(GameState game, Piece piece)
    {
        return new[] { GridVectors.Up };
    }
}