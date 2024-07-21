using Godot;
using Godot.Collections;

public partial class GridCell<[MustBeVariant] T>: Node where T : GridItem
{
    internal int x;
    internal int y;

    public Vector2I pos { get { return new Vector2I(x, y); } }

    private Grid<T> _grid;
    public Grid<T> grid { get { return _grid; } internal set
        {
            _grid = value;
            foreach (T item in items)
            {
                item.grid = value as Grid<GridItem>;
            }
        }
    }
    public Array<T> items { get; internal set; } = new Array<T>();

    public void SetPos(int x, int y)
    {
        // If position is the same, ignore
        if (this.x == x && this.y == y) return;

        this.x = x;
        this.y = y;
        // Update all items, as they have "moved" from one cell to another
        foreach (T item in items)
        {
            item.cell = this as GridCell<GridItem>;
        }
    }

    public bool HasItem(T item)
    {
        return items.Contains(item);
    }

    public int ItemCount()
    {
        return items.Count;
    }



    public bool AddItem(T item)
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
        AddChild(item);
        item.cell = this as GridCell<GridItem>;
        item.grid = grid as Grid<GridItem>;
        return true;
    }

    public T GetItem(int index)
    {
        return items[index];
    }

    public bool TryGet(int index, out GridItem item)
    {
        if (index >= 0 && index < items.Count)
        {
            item = items[index];
            return true;
        }
        item = null;
        return false;
    }

    internal bool RemoveItem(T item, bool updateItem)
    {
        if (!items.Remove(item))
        {
            return false;
        }
        RemoveChild(item);
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

    public bool RemoveItem(T item)
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
