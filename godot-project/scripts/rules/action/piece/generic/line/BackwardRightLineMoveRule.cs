using Godot;

public partial class BackwardRightLineMoveRule : LineMoveRule
{
    public override Vector2I[] GetDirs(GameState game, Piece piece)
    {
        return new[] { piece.forwardDirection.RotateClockwise(3).AsVector() };
    }
}