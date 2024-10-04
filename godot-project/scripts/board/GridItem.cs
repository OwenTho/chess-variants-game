using Godot;

public abstract partial class GridItem<T> : Node {
  [Signal]
  public delegate void ChangedCellEventHandler(GridCell<T> cell);

  public Grid<T> Grid { get; internal set; }

  // public Tags tag { get; private set; } = new Tags();
  public GridCell<T> Cell {
    get => _myCell;
    internal set {
      _myCell = value;
      CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.ChangedCell, value);
    }
  }

  private GridCell<T> _myCell;
}