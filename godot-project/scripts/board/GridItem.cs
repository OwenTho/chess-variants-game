using Godot;

public abstract partial class GridItem<T> : Node
{
    public Grid<T> grid { get; internal set; }
    private GridCell<T> myCell;
    // public Tags tag { get; private set; } = new Tags();
    public GridCell<T> cell {
        get { 
            return myCell;
        }
        internal set {
            myCell = value;
            EmitSignal(SignalName.ChangedCell, value);
        }
    }

    [Signal]
    public delegate void ChangedCellEventHandler(GridCell<T> cell);
    
}
