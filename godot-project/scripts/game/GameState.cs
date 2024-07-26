using System;
using Godot;
using Godot.Collections;

public partial class GameState : Node, ICloneable
{
    private GameController gameController;
    
    public Grid<GameItem> grid;
    public Grid<ActionBase> actionGrid;
    
    private int lastId = 0;
    public int currentPlayerNum;

    public const string KingId = "king";

    public Vector2I gridSize;

    private Array<Piece> allPieces;

    public GameState(GameController gameController)
    {
        this.gameController = gameController;
    }

    internal void Init()
    {
        lastId = 0;
        grid = new Grid<GameItem>();
        AddChild(grid);
        actionGrid = new Grid<ActionBase>();
        AddChild(actionGrid);
        allPieces = new Array<Piece>();
        gridSize = new Vector2I(8, 8);
    }

    public void SetPlayerNum(int newPlayerNum)
    {
        currentPlayerNum = newPlayerNum;
        currentPlayerNum %= GameController.NUMBER_OF_PLAYERS;
        if (currentPlayerNum < 0)
        {
            currentPlayerNum = 0;
        }
    }
    
    public Piece PlacePiece(string pieceId, int linkId, int teamId, int x, int y, int id = -1)
    {
        PieceInfo info = gameController.pieceInfoRegistry.GetValue(pieceId);
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
        newPiece.linkId = linkId;
        newPiece.teamId = teamId;

        newPiece.forwardDirection = gameController.GetTeamDirection(teamId);

        grid.PlaceItemAt(newPiece, x, y);

        return newPiece;
    }
    
    public void TakePiece(Piece piece)
    {
        // Remove piece from board
        grid.RemoveItem(piece);

        // Move to takenPieces
        allPieces.Remove(piece);

        // Free the Godot data
        piece.QueueFree();

        // Remove the actions from the grid
        foreach (ActionBase action in piece.currentPossibleActions)
        {
            action.QueueFree();
            action.cell.RemoveItem(action);
        }
        // Emit signal
        gameController.EmitSignal(GameController.SignalName.PieceRemoved, piece);
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
        if (grid.TryGetCellAt(x, y, out GridCell<GameItem> cell)) {
            foreach (var item in cell.items)
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
        if (grid.TryGetCellAt(x, y, out GridCell<GameItem> cell))
        {
            foreach (var item in cell.items)
            {
                if (item is Piece)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool HasPieceIdAt(string pieceId, int x, int y)
    {
        if (grid.TryGetCellAt(x, y, out GridCell<GameItem> cell))
        {
            foreach (var item in cell.items)
            {
                if (item is Piece)
                {
                    Piece piece = (Piece)item;
                    // If no info, ignore
                    if (piece.info == null)
                    {
                        continue;
                    }
                    // If piece id matches, return true
                    if (piece.info.pieceId == pieceId)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
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

    public Array<Piece> GetPiecesAt(int x, int y)
    {
        Array<Piece> pieces = new Array<Piece>();
        if (grid.TryGetCellAt(x, y, out GridCell<GameItem> cell))
        {
            foreach (var item in cell.items)
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
        
        bool didAct = false;
        // Loop through all actions, and find the ones at x, y.
        foreach (ActionBase action in possibleActions)
        {
            if (action.actionLocation == actionLocation)
            {
                didAct |= TakeAction(action, piece);
            }
        }
        return didAct;
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
            bool addToGrid = piece.needsActionUpdate;
            // Update all the actions
            piece.UpdateActions(this);
            // Add all the actions to the Grid, if they need to be.
            if (addToGrid)
            {
                foreach (ActionBase action in piece.currentPossibleActions)
                {
                    actionGrid.PlaceItemAt(action, action.actionLocation.X, action.actionLocation.Y);
                }
            }

            piece.NewTurn(this);
        }

        // Loop again, to disable certain check moves
        foreach (Piece piece in allPieces)
        {
            // If it's the king, disable any attacks on it and stop it from moving into
            // a space with check
            if (piece.info.pieceId != KingId)
            {
                continue;
            }
            
            if (actionGrid.TryGetCellAt(piece.cell.x, piece.cell.y, out GridCell<ActionBase> cell))
            {
                foreach (GridItem<ActionBase> item in cell.items)
                {
                    if (item is AttackAction)
                    {
                        AttackAction attackAction = (AttackAction)item;
                        attackAction.MakeInvalid();
                        if (attackAction.moveAction != null)
                        {
                            attackAction.moveAction.MakeInvalid();
                        }
                    }
                }
            }

            foreach (ActionBase action in piece.currentPossibleActions)
            {
                if (action is not MoveAction moveAction)
                {
                    continue;
                }

                foreach (var item in moveAction.cell.items)
                {
                    if (item is not AttackAction attackAction)
                    {
                        continue;
                    }

                    // Ignore actions that can't check
                    if (attackAction.verifyTags.Contains("no_check"))
                    {
                        continue;
                    }

                    if (attackAction.owner.teamId != piece.teamId)
                    {
                        moveAction.MakeInvalid();
                    }
                }
            }
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
        gameController.EmitSignal(GameController.SignalName.EndTurn);
        // Tell all pieces that it's the next turn
        foreach (Piece piece in allPieces)
        {
            // Tell the piece it's the end of the turn. It's
            // possible that the piece will request actions to
            // be remade as a result.
            piece.EndTurn(this);
            // Remove all actions from the Grid if the piece needs updating
            if (piece.needsActionUpdate)
            {
                foreach (ActionBase action in piece.currentPossibleActions)
                {
                    if (action.cell != null)
                    {
                        action.cell.RemoveItem(action);
                    }
                }

                piece.ClearActions();
            }
        }

        // Move to the next player
        if (newPlayerNum <= -1)
        {
            newPlayerNum = currentPlayerNum + 1;
        }
        SetPlayerNum(newPlayerNum);

        // Tell all pieces that it's the next turn
        PiecesNewTurn();
        gameController.EmitSignal(GameController.SignalName.NewTurn, currentPlayerNum);
    }

    public void NextTurn()
    {
        NextTurn(-1);
    }
    
    
    

    public object Clone()
    {
        // Initialise the new state
        GameState newState = new(gameController);
        newState.Init();

        // Copy over the pieces and their actions
        foreach (var piece in allPieces)
        {
            Piece newPiece = (Piece)piece.Clone();
            newState.allPieces.Add(newPiece);
            newState.grid.PlaceItemAt(newPiece, piece.cell.x, piece.cell.y);
            foreach (var action in piece.currentPossibleActions)
            {
                ActionBase newAction = (ActionBase)action.Clone();
                newPiece.AddAction(newAction);
                newState.actionGrid.PlaceItemAt(newAction, action.cell.x, action.cell.y);
            }
        }
        
        // Copy over variables
        newState.currentPlayerNum = currentPlayerNum;
        newState.gridSize = gridSize;

        return newState;
    }
}