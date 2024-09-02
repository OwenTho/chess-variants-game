
using Godot;

public enum PieceDirection
{
    Up,
    Down,
    Left,
    Right,
    None
}

static class PieceDirectionMethods
{
    public static Vector2I AsVector(this PieceDirection dir)
    {
        switch (dir)
        {
            case PieceDirection.Down:
                return GridVectors.Down;
            case PieceDirection.Up:
                return GridVectors.Up;
            case PieceDirection.Left:
                return GridVectors.Left;
            case PieceDirection.Right:
                return GridVectors.Right;
            case PieceDirection.None:
                return GridVectors.Zero;
        }
        return GridVectors.Zero;
    }
}