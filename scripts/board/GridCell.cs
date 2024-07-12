using Godot;
using Godot.Collections;

public partial class GridCell: GodotObject
{
    internal int x;
    internal int y;

    public Grid grid { get; internal set; }
    public Array<GridItem> items { get; internal set; } = new Array<GridItem>();

    public void SetPos(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public bool HasItem(GridItem item)
    {
        return items.Contains(item);
    }

    public int ItemCount()
    {
        return items.Count;
    }



    internal bool AddItem(GridItem item)
    {
        if (HasItem(item))
        {
            return false;
        }
        items.Add(item);
        item.cell = this;
        return true;
    }

    internal bool RemoveItem(GridItem item)
    {
        if (!HasItem(item))
        {
            return false;
        }
        items.Remove(item);
        item.cell = null;
        item.grid = null;
        return true;
    }

    internal void RemovedFromGrid()
    {
        grid = null;
    }
}
