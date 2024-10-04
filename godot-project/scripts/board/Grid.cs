using Godot;
using Godot.Collections;

public partial class Grid<T> : Node {
  public Array<GridCell<T>> Cells { get; private set; } = new();
  private Dictionary<Vector2I, GridCell<T>> _cellDict = new();

  // Get a cell at a specific location
  public bool TryGetCellAt(Vector2I pos, out GridCell<T> cell) {
    return _cellDict.TryGetValue(pos, out cell);
  }

  public bool TryGetCellAt(int x, int y, out GridCell<T> cell) {
    return TryGetCellAt(new Vector2I(x, y), out cell);
  }

  public GridCell<T> GetCellAt(Vector2I pos) {
    if (TryGetCellAt(pos, out GridCell<T> cell)) {
      return cell;
    }

    return null;
  }

  public GridCell<T> GetCellAt(int x, int y) {
    return GetCellAt(new Vector2I(x, y));
  }


  // Get a cell by the item held in it
  public GridCell<T> GetCellByItem(GridItem<T> item) {
    if (item == null) {
      return null;
    }

    foreach (GridCell<T> cell in Cells) {
      if (cell.HasItem(item)) {
        return cell;
      }
    }

    return null;
  }

  // Check if this grid holds a certain cell
  public bool HasCell(GridCell<T> cell) {
    if (cell == null) {
      return false;
    }

    // Use dictionary to look at the cell's position, so that it doesn't have
    // to check through all cells on the Grid.
    return IsCellAtCellPos(cell);
  }


  // Check if the grid has a cell at a certain position
  public bool HasCellAt(Vector2I pos) {
    return _cellDict.ContainsKey(pos);
  }

  public bool HasCellAt(int x, int y) {
    return HasCellAt(new Vector2I(x, y));
  }

  public bool RemoveCell(GridCell<T> cell) {
    // Only remove if it's on this grid
    if (cell == null || cell.Grid != this) {
      return false;
    }

    // Remove from the dictionary
    RemoveFromDict(cell.Pos);
    // Tell cell to remove its grid values
    cell.RemoveFromGrid();
    // If cell is removed, free it. This is because only
    // the Grid should use GridCell, so if it's removed then
    // it should be removed from memory.
    cell.CallDeferred(Node.MethodName.QueueFree);
    return Cells.Remove(cell);
  }

  public bool RemoveCellAt(Vector2I pos) {
    if (TryGetCellAt(pos, out GridCell<T> cell)) {
      RemoveCell(cell);
      return true;
    }

    return false;
  }

  public bool RemoveCellAt(int x, int y) {
    return RemoveCellAt(new Vector2I(x, y));
  }

  public int CellCount() {
    return Cells.Count;
  }

  public int ItemCount() {
    int count = 0;
    foreach (GridCell<T> cell in Cells) {
      count += cell.ItemCount();
    }

    return count;
  }

  public bool HasItem(GridItem<T> item) {
    // Only continue if the item is on this grid
    if (item == null || ReferenceEquals(item.Grid, this)) {
      return false;
    }

    foreach (GridCell<T> cell in Cells) {
      if (cell.HasItem(item)) {
        return true;
      }
    }

    return false;
  }

  public bool RemoveItem(GridItem<T> item) {
    // Only continue if the item is on this grid
    if (item == null || !ReferenceEquals(item.Grid, this)) {
      return false;
    }

    foreach (GridCell<T> cell in Cells) {
      if (cell.HasItem(item)) {
        cell.RemoveItem(item);
        return true;
      }
    }

    return false;
  }

  public GridCell<T> MakeNewCellAt(Vector2I pos) {
    // Remove any previous cell on this spot
    if (TryGetCellAt(pos, out GridCell<T> cell)) {
      RemoveCell(cell);
    }

    return NewCellAt(pos);
  }

  public GridCell<T> MakeNewCellAt(int x, int y) {
    return MakeNewCellAt(new Vector2I(x, y));
  }

  public GridCell<T> MakeOrGetCellAt(Vector2I pos) {
    // Return a cell if it's already there
    if (TryGetCellAt(pos, out GridCell<T> cell)) {
      return cell;
    }

    return NewCellAt(pos);
  }

  public GridCell<T> MakeOrGetCellAt(int x, int y) {
    return MakeOrGetCellAt(new Vector2I(x, y));
  }


  // Place an item at a position
  public GridCell<T> PlaceItemAt(GridItem<T> item, Vector2I pos, bool setGrid = true) {
    // Ignore if item is null
    if (item == null) {
      return null;
    }

    // If a cell already exists, then...
    if (TryGetCellAt(pos, out GridCell<T> cell)) {
      // Check if the item is already on this cell
      if (ReferenceEquals(cell, item.Cell)) {
        return cell;
      }

      // If the item is on another grid, remove it from that grid
      if (setGrid && item.Grid != null && !ReferenceEquals(item.Grid, this)) {
        item.Grid.RemoveItem(item);
      }

      // Now add the item to the cell.
      cell.AddItem(item, setGrid);
      return cell;
    }

    // If there isn't a cell there, then move the cell the item is in
    // (if it is in on) if it's already on this grid.
    if (ReferenceEquals(item.Grid, this) && item.Cell != null)
      // However, only move if it's the only item on this GridCell
    {
      if (item.Cell.ItemCount() == 1) {
        item.Cell.SetPos(pos);
        return item.Cell;
      }
    }

    // If it's on another grid, remove it.
    if (setGrid && item.Grid != null && !ReferenceEquals(item.Grid, this)) {
      item.Grid.RemoveItem(item);
    }

    // If none of the above puts the item onto a cell,
    // make a new cell for the item.
    GridCell<T> newCell = MakeNewCellAt(pos);
    newCell.AddItem(item, setGrid);
    return newCell;
  }

  public GridCell<T> PlaceItemAt(GridItem<T> item, int x, int y, bool setGrid = true) {
    return PlaceItemAt(item, new Vector2I(x, y), setGrid);
  }

  public void SwapCells(GridCell<T> cell1, GridCell<T> cell2) {
    // If either is null, then remove both from the grid.
    // It's swapping a game with nothing.
    if (cell1 == null || cell2 == null) {
      RemoveCell(cell1);
      RemoveCell(cell2);
      return;
    }

    // If either cell is on another grid, throw an error.
    if (cell1.Grid != this || cell2.Grid != this) {
      GD.PushError("Cannot swap cells on different grids.");
      return;
    }

    Vector2I pos1 = cell1.Pos;
    // Set their position directly, rather than using SetPos as
    // it will instead overwrite the existing cell.
    RemoveFromDict(cell1.Pos);
    RemoveFromDict(cell2.Pos);
    cell1.GridPos = cell2.Pos;
    cell2.GridPos = pos1;
    _cellDict.Add(cell1.Pos, cell1);
    _cellDict.Add(cell2.Pos, cell2);
  }

  public void SwapCellsAt(Vector2I pos1, Vector2I pos2) {
    // If same position, ignore.
    if (pos1 == pos2) {
      return;
    }

    GridCell<T> cell1 = GetCellAt(pos1);
    GridCell<T> cell2 = GetCellAt(pos2);

    // If both the same, ignore. This accounts for 2 nulls.
    if (cell1 == cell2) {
      return;
    }

    SwapCells(cell1, cell2);
  }

  public void SwapCellsAt(int x1, int y1, int x2, int y2) {
    SwapCellsAt(new Vector2I(x1, y1), new Vector2I(x2, y2));
  }


  internal bool RemoveFromDict(Vector2I pos) {
    if (HasCellAt(pos)) {
      _cellDict.Remove(pos);
      return true;
    }

    return false;
  }

  // Update a cell's position in the dictionary
  internal void UpdateCellPos(GridCell<T> cell, Vector2I newPos) {
    RemoveFromDict(cell.Pos);
    _cellDict.Add(newPos, cell);
    cell.GridPos = newPos;
  }

  internal bool IsCellAtCellPos(GridCell<T> cell) {
    if (!TryGetCellAt(cell.Pos, out GridCell<T> cellAtPos)) {
      return false;
    }

    return cell == cellAtPos;
  }


  private GridCell<T> NewCellAt(Vector2I pos) {
    GridCell<T> newCell = new();
    newCell.Grid = this;
    newCell.GridPos = pos;
    Cells.Add(newCell);
    _cellDict.Add(pos, newCell);
    CallDeferred(Node.MethodName.AddChild, newCell);
    return newCell;
  }
}