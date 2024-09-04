using Godot;

public partial class GameState
{
    [Signal]
    public delegate void NewTurnEventHandler(int newPlayerNum);

    [Signal]
    public delegate void ActionProcessedEventHandler(ActionBase action, Piece piece);
    
    [Signal]
    public delegate void ActionsProcessedAtEventHandler(bool success, Vector2I actionLocation, Piece piece);

    [Signal]
    public delegate void EndTurnEventHandler();

    [Signal]
    public delegate void PlayerLostEventHandler(int playerNum);

    [Signal]
    public delegate void GameStalemateEventHandler(int stalematePlayer);

    [Signal]
    public delegate void PieceRemovedEventHandler(Piece removedPiece, Piece attackerPiece);

    [Signal]
    public delegate void SendNoticeEventHandler(int playerTarget, string text);
    
    [Signal]
    public delegate void UpperBoundChangedEventHandler(Vector2I newBound);

    [Signal]
    public delegate void LowerBoundChangedEventHandler(Vector2I newBound);
}
