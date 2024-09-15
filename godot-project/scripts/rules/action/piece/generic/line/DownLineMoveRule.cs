using Godot;

public partial class DownLineMoveRule : LineMoveRule
{
    public override Vector2I[] GetDirs(GameState game, Piece piece)
    {
        return new[] { GridVectors.Down };
    }
}