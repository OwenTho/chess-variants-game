using Godot;

public partial class LeftLineMoveRule : LineMoveRule
{
    public override Vector2I[] GetDirs(GameState game, Piece piece)
    {
        return new[] { piece.forwardDirection.RotateAntiClockwise(2).AsVector() };
    }
}