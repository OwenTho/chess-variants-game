using Godot;
using System.Collections.Generic;

public partial class Grid : GodotObject
{
    internal List<GridCell> gridCells = new List<GridCell>();

    // Get a cell at a specific location
    public GridCell GetCellAt(int x, int y)
    {
        foreach (GridCell cell in gridCells)
        {
            if (cell.x == x && cell.y == y)
            {
                return cell;
            }
        }
        return null;
    }

    public bool TryGetCellAt(int x, int y, out GridCell cell)
    {
        cell = GetCellAt(x, y);
        return cell != null;
    }

    // Get a cell by the item held in it
    public GridCell GetCellByItem(GridItem item)
    {
        if (item == null)
        {
            return null;
        }

        foreach (GridCell cell in gridCells)
        {
            if (cell.HasItem(item))
            {
                return cell;
            }
        }
        return null;
    }


    // Check if this grid holds a certain cell
    public bool HasCell(GridCell cell)
    {
        if (cell == null)
        {
            return false;
        }

        return gridCells.Contains(cell);
    }

    // Check if the grid has a cell at a certain position
    public bool HasCellAt(int x, int y)
    {
        return GetCellAt(x, y) != null;
    }

    public bool RemoveCell(GridCell cell)
    {
        // Only remove if it's on this grid
        if (cell == null || cell.grid != this)
        {
            return false;
        }
        cell.RemoveFromGrid();
        return gridCells.Remove(cell);
    }

    public bool RemoveCellAt(int x, int y)
    {
        if (TryGetCellAt(x, y, out GridCell cell))
        {
            RemoveCell(cell);
            return true;
        }
        return false;
    }

    public int CellCount()
    {
        return gridCells.Count;
    }

    public bool HasItem(GridItem item)
    {
        // Only continue if the item is on this grid
        if (item == null || item.grid != this)
        {
            return false;
        }
        
        foreach (GridCell cell in gridCells)
        {
            if (cell.HasItem(item))
            {
                return true;
            }
        }
        return false;
    }

    public bool RemoveItem(GridItem item)
    {
        // Only continue if the item is on this grid
        if (item == null || item.grid != this)
        {
            return false;
        }

        foreach(GridCell cell in gridCells)
        {
            if (cell.HasItem(item))
            {
                cell.RemoveItem(item);
                // If the cell has no more items, remove it
                if (cell.ItemCount() == 0)
                {
                    RemoveCell(cell);
                }
                return true;
            }
        }
        return false;
    }

    internal GridCell NewCellAt(int x, int y)
    {
        GridCell newCell = new GridCell();
        newCell.SetPos(x, y);
        newCell.grid = this;
        gridCells.Add(newCell);
        return newCell;
    }

    public GridCell MakeCellAt(int x, int y)
    {
        // Remove any previous cell on this spot
        if (TryGetCellAt(x, y, out GridCell cell))
        {
            RemoveCell(cell);
        }
        return NewCellAt(x, y);
    }

    public GridCell MakeOrGetCellAt(int x, int y)
    {
        // Return a cell if it's already there
        if (TryGetCellAt(x, y, out GridCell cell))
        {
            return cell;
        }
        return NewCellAt(x, y);
    }

    
    public GridCell PlaceItemAt(GridItem item, int x, int y)
    {
        // Ignore if item is null
        if (item == null)
        {
            return null;
        }

        // If a cell already exists, then...
        if (TryGetCellAt(x, y, out GridCell cell))
        {
            // Check if the item is already on this cell
            if (cell == item.cell)
            {
                return cell;
            }

            // If the item is on another grid, remove it from that grid
            if (item.grid != null)
            {
                item.grid.RemoveItem(item);
            }
            // Now add the item to the cell.
            cell.AddItem(item);
            return cell;
        }

        // If there isn't a cell there, then move the cell the item is in
        // (if it is in on) if it's already on this grid.
        if (item.grid == this && item.cell != null)
        {
            // However, only move if it's the only item on this GridCell
            if (item.cell.ItemCount() == 1)
            {
                item.cell.SetPos(x, y);
                return item.cell;
            }
        }

        // If it's on a grid, remove it.
        // This applies to the current grid too.
        if (item.grid != null)
        {
            item.grid.RemoveItem(item);
        }

        // If none of the above puts the item onto a cell,
        // make a new cell for the item.
        GridCell newCell = MakeCellAt(x, y);
        newCell.AddItem(item);
        return newCell;
    }

    public void SwapCells(GridCell cell1, GridCell cell2)
    {
        // If either is null, then remove both from the grid.
        // It's swapping a game with nothing.
        if (cell1 == null || cell2 == null)
        {
            RemoveCell(cell1);
            RemoveCell(cell2);
            return;
        }

        // If either cell is on another grid, throw an error.
        if ((cell1 != null && cell1.grid != this) || (cell2 != null && cell2.grid != this))
        {
            GD.PushError("Cells should be on this grid if non-null.");
            return;
        }

        int x1 = cell1.x;
        int y1 = cell1.y;
        cell1.SetPos(cell2.x, cell2.y);
        cell2.SetPos(x1, y1);
    }

    public void SwapCellsAt(int x1, int y1, int x2, int y2)
    {
        // If same position, ignore.
        if (x1 == x2 && y1 == y2)
        {
            return;
        }
        GridCell cell1 = GetCellAt(x1, y1);
        GridCell cell2 = GetCellAt(x2, y2);

        // If both the same, ignore. This accounts for 2 nulls.
        if (cell1 == cell2)
        {
            return;
        }

        SwapCells(cell1, cell2);
    }
}
