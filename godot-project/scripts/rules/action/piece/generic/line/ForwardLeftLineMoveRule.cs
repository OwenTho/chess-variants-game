using Godot;

public partial class ForwardLeftLineMoveRule : LineMoveRule
{
    public override Vector2I[] GetDirs(GameState game, Piece piece)
    {
        return new[] { piece.forwardDirection.RotateAntiClockwise(1).AsVector() };
    }
}