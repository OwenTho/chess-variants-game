using Godot;

public partial class BackwardLineMoveRule : LineMoveRule
{
    public override Vector2I[] GetDirs(GameState game, Piece piece)
    {
        return new[] { piece.forwardDirection.RotateClockwise(4).AsVector() };
    }
}