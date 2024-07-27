using Godot;

public partial class GameController : Node
{
    [Signal]
    public delegate void NewTurnEventHandler(int newPlayerNum);

    [Signal]
    public delegate void PieceRemovedEventHandler(Piece removedPiece);

    [Signal]
    public delegate void RequestedActionAtEventHandler(Vector2I actionLocation, Piece piece);

    [Signal]
    public delegate void EndTurnEventHandler();
    
    [Signal]
    public delegate void SendNoticeEventHandler(int playerTarget, string text);
}
