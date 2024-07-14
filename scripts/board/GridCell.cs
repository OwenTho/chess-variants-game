using Godot;
using System.Collections.Generic;

public partial class GridCell: GodotObject
{
    internal int x;
    internal int y;

    public Grid grid { get; internal set; }
    public List<GridItem> items { get; internal set; } = new List<GridItem>();

    public void SetPos(int x, int y)
    {
        // If position is the same, ignore
        if (this.x == x && this.y == y) return;

        this.x = x;
        this.y = y;
        // Update all items, as they have "moved" from one cell to another
        foreach (GridItem item in items)
        {
            item.cell = this;
        }
    }

    public bool HasItem(GridItem item)
    {
        return items.Contains(item);
    }

    public int ItemCount()
    {
        return items.Count;
    }



    public bool AddItem(GridItem item)
    {
        // If item already has a cell, and it's not this one,
        // then remove it from that cell.
        if (item.cell != null)
        {
            item.cell.RemoveItem(item);
        }
        if (HasItem(item))
        {
            return false;
        }
        items.Add(item);
        item.cell = this;
        item.grid = grid;
        return true;
    }

    public bool RemoveItem(GridItem item)
    {
        if (!items.Remove(item))
        {
            return false;
        }
        item.cell = null;
        item.grid = null;
        // If this cell has no more items, remove it from the grid
        if (ItemCount() == 0)
        {
            grid.RemoveCell(this);
        }
        return true;
    }

    internal void RemoveFromGrid()
    {
        grid = null;
    }
}
