
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
                return Vector2I.Up; // This is -Y
            case PieceDirection.Up:
                return Vector2I.Down; // This is +Y
            case PieceDirection.Left:
                return Vector2I.Left;
            case PieceDirection.Right:
                return Vector2I.Right;
            case PieceDirection.None:
                return Vector2I.Zero;
        }
        return Vector2I.Zero;
    }
}