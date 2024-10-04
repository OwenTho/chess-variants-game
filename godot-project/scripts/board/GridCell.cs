using Godot;
using Godot.Collections;

public partial class GridCell<T> : Node {
  public Vector2I Pos {
    get => GridPos;
    private set => SetPos(value);
  }

  public int X => Pos.X;
  public int Y => Pos.Y;

  public Grid<T> Grid {
    get => _grid;
    internal set {
      _grid = value;
      foreach (GridItem<T> item in Items) {
        item.Grid = value;
      }
    }
  }

  public Array<GridItem<T>> Items { get; internal set; } = new();


  internal Vector2I GridPos;
  private Grid<T> _grid;

  /**
   * Updates the position of the cell.
   * <br></br>
   * <br></br>
   * Setting it to the position of another cell will remove that other cell from the Grid.
   */
  public void SetPos(Vector2I newPos) {
    // If position is the same, ignore
    if (GridPos == newPos) {
      return;
    }

    Grid.UpdateCellPos(this, newPos);
    // Update all items, as they have "moved" from one cell to another
    foreach (GridItem<T> item in Items) {
      item.Cell = this;
    }
  }

  public void SetPos(int newX, int newY) {
    SetPos(new Vector2I(newX, newY));
  }

  public bool HasItem(GridItem<T> item) {
    return Items.Contains(item);
  }

  public int ItemCount() {
    return Items.Count;
  }


  public bool AddItem(GridItem<T> item, bool setGrid = true) {
    // If item already has a cell, and it's not this one,
    // then remove it from that cell.
    if (setGrid && item.Cell != null)
      // Don't update the item, as it will be updated on being added.
    {
      item.Cell.RemoveItem(item, false);
    }

    if (HasItem(item)) {
      return false;
    }

    Items.Add(item);
    if (setGrid) {
      CallDeferred(Node.MethodName.AddChild, item);
      item.Cell = this;
      item.Grid = Grid;
    }

    return true;
  }

  public GridItem<T> GetItem(int index) {
    return Items[index];
  }

  public bool TryGet(int index, out GridItem<T> item) {
    if (index >= 0 && index < Items.Count) {
      item = Items[index];
      return true;
    }

    item = null;
    return false;
  }

  public bool RemoveItem(GridItem<T> item) {
    return RemoveItem(item, true);
  }


  private bool RemoveItem(GridItem<T> item, bool updateItem) {
    if (!Items.Remove(item)) {
      return false;
    }

    CallDeferred(Node.MethodName.RemoveChild, item);
    if (updateItem) {
      item.Cell = null;
      item.Grid = null;
    }

    // If this cell has no more items, remove it from the grid
    if (Grid != null && ItemCount() == 0) {
      Grid.RemoveCell(this);
    }

    return true;
  }

  internal void RemoveFromGrid() {
    Grid = null;
    // Remove all items, too.
    foreach (GridItem<T> item in Items) {
      RemoveItem(item);
    }
  }
}