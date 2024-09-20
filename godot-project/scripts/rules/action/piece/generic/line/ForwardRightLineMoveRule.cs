using Godot;

public partial class ForwardRightLineMoveRule : LineMoveRule
{
    public override Vector2I[] GetDirs(GameState game, Piece piece)
    {
        return new[] { piece.forwardDirection.RotateClockwise(1).AsVector() };
    }
}