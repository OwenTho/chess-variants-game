
using Godot;

public enum PieceDirection
{
    Right,
    
    DownRight,
    Down,
    DownLeft,
    
    Left,
    
    UpLeft,
    Up,
    UpRight,
    
    None
}

static class PieceDirectionMethods
{
    public static Vector2I AsVector(this PieceDirection dir)
    {
        switch (dir)
        {
            case PieceDirection.Right:
                return GridVectors.Right;
            
            case PieceDirection.DownRight:
                return GridVectors.DownRight;
            case PieceDirection.Down:
                return GridVectors.Down;
            case PieceDirection.DownLeft:
                return GridVectors.DownLeft;
            
            case PieceDirection.Left:
                return GridVectors.Left;
            
            case PieceDirection.UpLeft:
                return GridVectors.UpLeft;
            case PieceDirection.Up:
                return GridVectors.Up;
            case PieceDirection.UpRight:
                return GridVectors.UpRight;
            
            case PieceDirection.None:
                return GridVectors.Zero;
        }
        return GridVectors.Zero;
    }

    public static int ToInt(this PieceDirection dir)
    {
        switch (dir)
        {
            case PieceDirection.Right:
                return 0;
            
            case PieceDirection.DownRight:
                return 1;
            case PieceDirection.Down:
                return 2;
            case PieceDirection.DownLeft:
                return 3;
            
            case PieceDirection.Left:
                return 4;
            
            case PieceDirection.UpLeft:
                return 5;
            case PieceDirection.Up:
                return 6;
            case PieceDirection.UpRight:
                return 7;
        }

        return 0;
    }

    public static PieceDirection FromInt(int dir)
    {
        switch (dir)
        {
            case 0:
                return PieceDirection.Right;
            
            case 1:
                return PieceDirection.DownRight;
            case 2:
                return PieceDirection.Down;
            case 3:
                return PieceDirection.DownLeft;
            
            case 4:
                return PieceDirection.Left;
            
            case 5:
                return PieceDirection.UpLeft;
            case 6:
                return PieceDirection.Up;
            case 7:
                return PieceDirection.UpRight;
        }

        return PieceDirection.None;
    }
    
    public static PieceDirection RotateClockwise(this PieceDirection dir, int rotations)
    {
        int val = dir.ToInt();
        val += rotations;
        // On negative rotations, add until it's positive
        if (rotations < 0)
        {
            while (val < 0)
            {
                val += 8;
            }
        }
        // On positive rotations, mod the value
        else
        {
            val %= 8;
        }

        return FromInt(val);
    }

    public static PieceDirection RotateAntiClockwise(this PieceDirection dir, int rotations)
    {
        return RotateClockwise(dir, -rotations);
    }
}