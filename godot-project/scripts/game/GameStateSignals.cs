using Godot;

public partial class GameState : Node
{
    [Signal]
    public delegate void NewTurnEventHandler(int newPlayerNum);

    [Signal]
    public delegate void PieceRemovedEventHandler(Piece removedPiece);

    [Signal]
    public delegate void EndTurnEventHandler();
}
