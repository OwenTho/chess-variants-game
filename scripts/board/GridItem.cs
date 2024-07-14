using Godot;

public partial class GridItem : GodotObject
{
    public Grid grid { get; internal set; }
    private GridCell myCell;
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
