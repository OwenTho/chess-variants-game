using Godot;

public partial class GameState
{
    [Signal]
    public delegate void NewTurnEventHandler(int newPlayerNum);
    
    [Signal]
    public delegate void ActionProcessedEventHandler(bool success, Vector2I actionLocation, Piece piece);

    [Signal]
    public delegate void EndTurnEventHandler();

    [Signal]
    public delegate void PlayerLostEventHandler(int playerNum);

    [Signal]
    public delegate void PieceRemovedEventHandler(Piece removedPiece, Piece attackerPiece);

    [Signal]
    public delegate void SendNoticeEventHandler(int playerTarget, string text);
}
