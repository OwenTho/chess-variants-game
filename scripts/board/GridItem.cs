using Godot;

public partial class GridItem : Node
{
    public Grid grid { get; internal set; }
    private GridCell myCell;
    // public Tags tag { get; private set; } = new Tags();
    public GridCell cell {
        get { 
            return myCell;
        }
        internal set {
            myCell = value;
            EmitSignal(SignalName.ChangedCell, value);
        }
    }

    [Signal]
    public delegate void ChangedCellEventHandler(GridCell cell);
}
