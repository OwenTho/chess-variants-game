using Godot;

public partial class LeftLineMoveRule : LineMoveRule
{
    public override Vector2I[] GetDirs(GameState game, Piece piece)
    {
        return new[] { GridVectors.Left };
    }
}