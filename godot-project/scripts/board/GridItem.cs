using Godot;

public partial class GridItem : Node
{
    public Grid<GridItem> grid { get; internal set; }
    private GridCell<GridItem> myCell;
    // public Tags tag { get; private set; } = new Tags();
    public GridCell<GridItem> cell {
        get { 
            return myCell;
        }
        internal set {
            myCell = value;
            EmitSignal(SignalName.ChangedCell, value);
        }
    }

    [Signal]
    public delegate void ChangedCellEventHandler(GridCell<GridItem> cell);
    
}
