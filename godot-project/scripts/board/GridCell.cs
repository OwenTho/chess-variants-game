using Godot;
using Godot.Collections;

public partial class GridCell<T> : Node
{
    internal int x;
    internal int y;

    public Vector2I pos { get { return new Vector2I(x, y); } }

    private Grid<T> _grid;
    public Grid<T> grid { get { return _grid; } internal set
        {
            _grid = value;
            foreach (var item in items)
            {
                item.grid = value;
            }
        }
    }
    public Array<GridItem<T>> items { get; internal set; } = new();

    public void SetPos(int x, int y)
    {
        // If position is the same, ignore
        if (this.x == x && this.y == y) return;

        this.x = x;
        this.y = y;
        // Update all items, as they have "moved" from one cell to another
        foreach (var item in items)
        {
            item.cell = this;
        }
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
