using Godot;

public partial class DownLeftLineMoveRule : LineMoveRule
{
    public override Vector2I[] GetDirs(GameState game, Piece piece)
    {
        return new[] { GridVectors.DownLeft };
    }
}