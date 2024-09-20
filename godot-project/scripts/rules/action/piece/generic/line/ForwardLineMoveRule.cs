using Godot;

public partial class ForwardLineMoveRule : LineMoveRule
{
    public override Vector2I[] GetDirs(GameState game, Piece piece)
    {
        return new[] { piece.forwardDirection.AsVector() };
    }
}