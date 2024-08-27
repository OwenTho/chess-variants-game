using Godot;
using Godot.Collections;

public partial class GridCell<T> : Node
{
    internal Vector2I _pos;

    public Vector2I pos
    {
        get => _pos;
        set => SetPos(value);
    }

    public int x => _pos.X;
    public int y => _pos.Y;
    

    private Grid<T> _grid;
    public Grid<T> grid { 
        get => _grid;
        internal set
        {
            _grid = value;
            foreach (var item in items)
            {
                item.grid = value;
            }
        }
    }
    public Array<GridItem<T>> items { get; internal set; } = new();

    /**
     * Updates the position of the cell.
     * <br></br><br></br>
     * Setting it to the position of another cell will remove that other cell from the Grid.
     */
    public void SetPos(Vector2I newPos)
    {
        // If position is the same, ignore
        if (_pos == newPos) return;

        grid.UpdateCellPos(this, newPos);
        // Update all items, as they have "moved" from one cell to another
        foreach (var item in items)
        {
            item.cell = this;
        }
    }
    
    public void SetPos(int newX, int newY)
    {
        SetPos(new Vector2I(newX, newY));
    }

    public bool HasItem(GridItem<T> item)
    {
        return items.Contains(item);
    }

    public int ItemCount()
    {
        return items.Count;
    }



    public bool AddItem(GridItem<T> item)
    {
        // If item already has a cell, and it's not this one,
        // then remove it from that cell.
        if (item.cell != null)
        {
            // Don't update the item, as it will be updated on being added.
            item.cell.RemoveItem(item, false);
        }
        if (HasItem(item))
        {
            return false;
        }
        items.Add(item);
        CallDeferred(Node.MethodName.AddChild, item);
        item.cell = this;
        item.grid = grid;
        return true;
    }

    public GridItem<T> GetItem(int index)
    {
        return items[index];
    }

    public bool TryGet(int index, out GridItem<T> item)
    {
        if (index >= 0 && index < items.Count)
        {
            item = items[index];
            return true;
        }
        item = null;
        return false;
    }

    internal bool RemoveItem(GridItem<T> item, bool updateItem)
    {
        if (!items.Remove(item))
        {
            return false;
        }
        CallDeferred(Node.MethodName.RemoveChild, item);
        if (updateItem)
        {
            item.cell = null;
            item.grid = null;
        }
        // If this cell has no more items, remove it from the grid
        if (grid != null && ItemCount() == 0)
        {
            grid.RemoveCell(this);
        }
        return true;
    }

    public bool RemoveItem(GridItem<T> item)
    {
        return RemoveItem(item, true);
    }

    internal void RemoveFromGrid()
    {
        grid = null;
        // Remove all items, too.
        foreach (var item in items)
        {
            RemoveItem(item);
        }
    }
}
