
public partial class GameEvents
{
    public const string StartGame = "start_game"; // Before the game starts
    public const string GameStarted = "game_started"; // Game has started
    
    public const string PreNewTurn = "pre_new_turn"; // Right before the new turn starts
    public const string NewTurn = "new_turn"; // New turn has started
    public const string EndTurn = "end_turn"; // Turn has ended
    
    public const string AttackPiece = "attack_piece"; // Piece will be attacked with an AttackAction (can be cancelled)
    public const string PieceTaken = "piece_taken"; // Piece has been taken
    
    public const string MovePiece = "move_piece"; // Piece will be moved with a MoveAction (can be cancelled)
    public const string PieceMoved = "piece_moved"; // Piece has been moved
}