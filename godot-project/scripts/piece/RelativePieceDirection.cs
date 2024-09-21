
public enum RelativePieceDirection : int
{
    BackwardLeft = -3,
    Left = -2,
    ForwardLeft = -1,
    Forward = 0,
    ForwardRight = 1,
    Right = 2,
    BackwardRight = 3,
    Backward = 4
}

public static class LinePieceDirectionMethods
{
    public static PieceDirection FromDir(this RelativePieceDirection dir, PieceDirection pieceDirection)
    {
        return pieceDirection.RotateClockwise((int) dir);
    }
}