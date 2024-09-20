using Godot;

public partial class BackwardLeftLineMoveRule : LineMoveRule
{
    public override Vector2I[] GetDirs(GameState game, Piece piece)
    {
        return new[] { piece.forwardDirection.RotateAntiClockwise(3).AsVector() };
    }
}