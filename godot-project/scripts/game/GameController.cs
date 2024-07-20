using Godot;
using Godot.Collections;
using System;
using System.Reflection.PortableExecutable;

public partial class GameController : Node
{
    public Grid grid { get; private set; }

    public Vector2I gridSize = new Vector2I(8, 8);

    private int lastId = 0;

    public const int NUMBER_OF_PLAYERS = 2;

    public int currentPlayerNum { get; private set; } = 0;

    public Array<Piece> allPieces = new Array<Piece>();

    public void SetPlayerNum(int newPlayerNum)
    {
        currentPlayerNum = newPlayerNum;
        currentPlayerNum %= NUMBER_OF_PLAYERS;
        if (currentPlayerNum < 0)
        {
            currentPlayerNum = 0;
        }
    }

    public Piece PlacePiece(string pieceId, int linkId, int teamId, int x, int y, int id = -1)
    {
        PieceInfo info = pieceInfoRegistry.GetValue(pieceId);
        if (info == null)
        {
            GD.PushWarning("Tried to place a piece with {pieceId}, even though it hasn't been registered!");
            return null;
        }

        
        Piece newPiece = new Piece();
        allPieces.Add(newPiece);
        newPiece.info = info;
        if (id != -1)
        {
            newPiece.id = id;
        } else
        {
            newPiece.id = lastId;
            lastId += 1;
        }
        lastId += 1;
        newPiece.linkId = linkId;
        newPiece.teamId = teamId;

        newPiece.forwardDirection = GetTeamDirection(teamId);

        grid.PlaceItemAt(newPiece, x, y);

        return newPiece;
    }

    public Piece GetPiece(int pieceId)
    {
        foreach (Piece piece in allPieces)
        {
            if (piece.id == pieceId)
            {
                return piece;
            }
        }
        return null;
    }

    public bool TryGetPiece(int pieceId, out Piece piece)
    {
        piece = GetPiece(pieceId);
        return piece != null;
    }

    public void TakePiece(Piece piece)
    {
        // Remove piece from board
        grid.RemoveItem(piece);

        // Remove from allPieces
        allPieces.Remove(piece);

        // Emit signal
        EmitSignal(SignalName.PieceRemoved, piece);
    }

    public Piece TakePiece(int pieceId)
    {
        if (TryGetPiece(pieceId, out Piece piece))
        {
            TakePiece(piece);
            return piece;
        }
        return null;
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


    public bool IsActionValid(ActionBase action, Piece piece)
    {
        // The piece must be of the current team
        if (piece == null || piece.teamId != currentPlayerNum)
        {
            // GD.Print($"Invalid {action.GetType().Name} ({action.actionLocation.X}, {action.actionLocation.Y}): Piece null or wrong team.");
            return false;
        }
        // Ignore if null
        if (action == null)
        {
            GD.PushError($"Tried to Act on piece {piece.GetType().Name}, but the Action was null.");
            return false;
        }
        // Ignore if invalid
        if (!action.valid)
        {
            // GD.Print($"Invalid {action.GetType().Name} ({action.actionLocation.X}, {action.actionLocation.Y}): Action is invalid: {action.tags}");
            return false;
        }
        return true;
    }

    public Piece GetFirstPieceAt(int x, int y)
    {
        if (grid.TryGetCellAt(x, y, out GridCell cell)) {
            foreach (GridItem item in cell.items)
            {
                if (item is Piece)
                {
                    return (Piece)item;
                }
            }
        }
        return null;
    }

    public bool TryGetFirstPieceAt(int x, int y, out Piece piece)
    {
        piece = GetFirstPieceAt(x, y);
        return piece != null;
    }

    public bool HasPieceAt(int x, int y)
    {
        if (grid.TryGetCellAt(x, y, out GridCell cell))
        {
            foreach (GridItem item in cell.items)
            {
                if (item is Piece)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public Array<Piece> GetPiecesAt(int x, int y)
    {
        Array<Piece> pieces = new Array<Piece>();
        if (grid.TryGetCellAt(x, y, out GridCell cell))
        {
            foreach (GridItem item in cell.items)
            {
                if (item is Piece)
                {
                    pieces.Add((Piece)item);
                }
            }
        }
        return pieces;
    }

    public bool TryGetPiecesAt(int x, int y, out Array<Piece> pieces)
    {
        pieces = GetPiecesAt(x, y);
        return pieces.Count > 0;
    }

    public bool TakeAction(ActionBase action, Piece piece)
    {
        if (!IsActionValid(action, piece))
        {
            return false;
        }
        
        action.ActOn(this, piece);
        return true;
    }

    public bool TakeActionAt(Vector2I actionLocation, Piece piece)
    {
        // Get the possible actions for this piece
        Array<ActionBase> possibleActions = piece.currentPossibleActions;
        if (possibleActions.Count == 0)
        {
            return false;
        }

        // Loop through all actions, and find the ones at x, y.
        foreach (ActionBase action in possibleActions)
        {
            if (action.actionLocation == actionLocation)
            {
                TakeAction(action, piece);
            }
        }
        return true;
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
        return;
    }

    public void RequestActionsAt(Vector2I actionLocation, Piece piece)
    {
        // The piece must be of the current team
        if (piece.teamId != currentPlayerNum)
        {
            return;
        }

        // Tell networking "I want to act this piece on this location"
        EmitSignal(SignalName.RequestedActionAt, actionLocation, piece);
    }

    public void StartGame()
    {
        // Tell all pieces to update their possible moves
        PiecesNewTurn();
    }

    private void PiecesNewTurn()
    {
        foreach (Piece piece in allPieces)
        {
            // GD.Print($"Updating for piece {piece.id}");
            // Update all the actions
            piece.UpdateActions(this);
            // Add all the actions to the Grid
            foreach (ActionBase action in piece.currentPossibleActions)
            {
                // GD.Print($"\t{action.GetType().Name}({action.actionLocation.X}, {action.actionLocation.Y}) - {action.valid}");
                grid.PlaceItemAt(action, action.actionLocation.X, action.actionLocation.Y);
            }
            piece.NewTurn(this);
        }
    }

    public void NextTurn(int newPlayerNum)
    {
        // If it's already this player's turn, ignore
        if (newPlayerNum == currentPlayerNum)
        {
            return;
        }
        // First, end turn
        EmitSignal(SignalName.EndTurn);
        // Tell all pieces that it's the next turn
        foreach (Piece piece in allPieces)
        {
            // Remove all actions from the Grid
            foreach (ActionBase action in piece.currentPossibleActions)
            {
                if (action.cell != null)
                {
                    action.cell.RemoveItem(action);
                }
            }

            piece.ClearActions();
            piece.EndTurn(this);
        }

        // Move to the next player
        if (newPlayerNum <= -1)
        {
            newPlayerNum = currentPlayerNum + 1;
        }
        SetPlayerNum(newPlayerNum);

        // Tell all pieces that it's the next turn
        PiecesNewTurn();
        EmitSignal(SignalName.NewTurn, currentPlayerNum);
    }

    public void NextTurn()
    {
        NextTurn(-1);
    }
}
