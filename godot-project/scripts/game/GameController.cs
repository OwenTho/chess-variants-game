using Godot;
using Godot.Collections;

public partial class GameController : Node
{
    public GameState currentGameState { get; private set; }

    public const int NUMBER_OF_PLAYERS = 2;

    public Piece PlacePiece(string pieceId, int linkId, int teamId, int x, int y, int id = -1)
    {
        return currentGameState.PlacePiece(pieceId, linkId, teamId, x, y, id);
    }

    public Piece GetPiece(int pieceId)
    {
        return currentGameState.GetPiece(pieceId);
    }

    public bool TryGetPiece(int pieceId, out Piece piece)
    {
        return currentGameState.TryGetPiece(pieceId, out piece);
    }

    public Vector2I GetTeamDirection(int teamId)
    {
        switch (teamId)
        {
            case 0:
                return Vector2I.Down;
            case 1:
                return Vector2I.Up;
        }
        return Vector2I.Zero;
    }

    public Array<string> GetPieceKeys()
    {
        string[] keys = pieceInfoRegistry.GetKeys();
        Array<string> pieceKeys = new Array<string>();
        foreach (string key in keys)
        {
            pieceKeys.Add(key);
        }
        return pieceKeys;
    }

    public PieceInfo GetPieceInfo(string key)
    {
        return pieceInfoRegistry.GetValue(key);
    }

    public RuleBase GetRule(string key)
    {
        return actionRuleRegistry.GetValue(key);
    }

    public void RequestAction(ActionBase action, Piece piece)
    {
        // Ignore if null
        if (action == null)
        {
            GD.PushError($"Tried to Act on piece {piece.GetType().Name}, but the Action was null.");
            return;
        }

        // Request at the location
        RequestActionsAt(action.actionLocation, piece);
    }

    public void RequestActionsAt(Vector2I actionLocation, Piece piece)
    {
        // The piece must be of the current team
        if (piece.teamId != currentGameState.currentPlayerNum)
        {
            return;
        }

        // Tell networking "I want to act this piece on this location"
        EmitSignal(SignalName.RequestedActionAt, actionLocation, piece);
    }
    
    
    
    
    // Function calls to the currentGameState for easier access
    
    public bool IsActionValid(ActionBase action, Piece piece)
    {
        return currentGameState.IsActionValid(action, piece);
    }

    public Piece GetFirstPieceAt(int x, int y)
    {
        return currentGameState.GetFirstPieceAt(x, y);
    }

    public bool TryGetFirstPieceAt(int x, int y, out Piece piece)
    {
        return currentGameState.TryGetFirstPieceAt(x, y, out piece);
    }

    public bool HasPieceAt(int x, int y)
    {
        return currentGameState.HasPieceAt(x, y);
    }

    public bool HasPieceIdAt(string pieceId, int x, int y)
    {
        return currentGameState.HasPieceIdAt(pieceId, x, y);
    }

    public Array<Piece> GetPiecesAt(int x, int y)
    {
        return currentGameState.GetPiecesAt(x, y);
    }

    public bool TryGetPiecesAt(int x, int y, out Array<Piece> pieces)
    {
        return currentGameState.TryGetPiecesAt(x, y, out pieces);
    }

    public bool TakeAction(ActionBase action, Piece piece)
    {
        return currentGameState.TakeAction(action, piece);
    }

    public bool TakeActionAt(Vector2I actionLocation, Piece piece)
    {
        return currentGameState.TakeActionAt(actionLocation, piece);
    }
}
