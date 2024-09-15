using Godot;

public partial class UpLeftLineMoveRule : LineMoveRule
{
    public override Vector2I[] GetDirs(GameState game, Piece piece)
    {
        return new[] { GridVectors.UpLeft };
    }
}