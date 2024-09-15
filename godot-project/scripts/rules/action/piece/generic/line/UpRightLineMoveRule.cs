using Godot;

public partial class UpRightLineMoveRule : LineMoveRule
{
    public override Vector2I[] GetDirs(GameState game, Piece piece)
    {
        return new[] { GridVectors.UpRight };
    }
}