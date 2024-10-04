using Godot;

public static class GridVectors {
  public static readonly Vector2I UpLeft = new(-1, 1);
  public static readonly Vector2I Up = new(0, 1);
  public static readonly Vector2I UpRight = new(1, 1);
  public static readonly Vector2I Left = new(-1, 0);
  public static readonly Vector2I Right = new(1, 0);
  public static readonly Vector2I DownLeft = new(-1, -1);
  public static readonly Vector2I Down = new(0, -1);
  public static readonly Vector2I DownRight = new(1, -1);

  public static readonly Vector2I Zero = new(0, 0);
}