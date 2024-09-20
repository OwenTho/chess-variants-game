using Godot;

public partial class RightLineMoveRule : LineMoveRule
{
    public override Vector2I[] GetDirs(GameState game, Piece piece)
    {
        return new[] { piece.forwardDirection.RotateClockwise(2).AsVector() };
    }
}