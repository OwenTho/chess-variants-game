using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class GameState : Node
{
    private GameController gameController;
    
    public Grid<GameItem> grid;
    public Grid<ActionBase> actionGrid;
    
    private int lastId = 0;
    public int currentPlayerNum;

    public const string KingId = "king";

    public Vector2I gridSize;

    private Array<Piece> allPieces;
    
    bool tempState = false;
    
    // If the GameState needs to do Check checks. This is enabled for the server,
    // and disabled for the clients.
    bool needToCheck = true;

    public enum CheckType
    {
        NONE,
        IN_CHECK,
        POSSIBLE_CHECKMATE
    }

    public CheckType[] playerCheck;

    public GameState(GameController gameController)
    {
        this.gameController = gameController;
    }

    internal void Init(bool needToCheck)
    {
        lastId = 0;
        grid = new Grid<GameItem>();
        CallDeferred(Node.MethodName.AddChild, grid);
        actionGrid = new Grid<ActionBase>();
        CallDeferred(Node.MethodName.AddChild, actionGrid);
        allPieces = new Array<Piece>();
        gridSize = new Vector2I(8, 8);

        playerCheck = new CheckType[GameController.NUMBER_OF_PLAYERS];
        for (int i = 0; i < playerCheck.Length; i++)
        {
            playerCheck[i] = CheckType.NONE;
        }

        this.needToCheck = needToCheck;
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
    
    public void TakePiece(Piece piece, Piece attacker = null)
    {
        // Remove it from the board
        grid.RemoveItem(piece);
        
        // Move to takenPieces
        allPieces.Remove(piece);
        
        // Free the item. This also frees the action data
        piece.QueueFree();
        
        // Remove the actions from the grid
        foreach (var action in piece.currentPossibleActions)
        {
            action.cell.RemoveItem(action);
            action.QueueFree();
        }
        
        // Emit signal
        CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.PieceRemoved, piece, attacker);
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

    public bool IsTargeted(Piece piece)
    {
        if (actionGrid.TryGetCellAt(piece.cell.x, piece.cell.y, out GridCell<ActionBase> cell))
        {
            foreach (var item in cell.items)
            {
                if (item is AttackAction attackAction)
                {
                    // If the attack is from another team, and valid, then return true
                    if (piece.teamId != attackAction.owner.teamId)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
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
        // Ignore if not acting
        if (!action.acting)
        {
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

    public bool DoesActionCheck(Vector2I actionLocation, Piece piece)
    {
        // If it's a temp state, return (to avoid recursion)
        if (tempState)
        {
            return playerCheck[piece.teamId] != CheckType.NONE;
        }
        // If it's non-check, ignore as server already did the check
        if (!needToCheck)
        {
            return false;
        }
        // Simulate the movement, and check if the player is still in check
        GameState newState = (GameState)Clone();
        CallDeferred(Node.MethodName.AddChild, newState);
        newState.tempState = true;
        
        // Do the actions, and go to the next turn
        bool actionWorked = newState.DoActionsAt(actionLocation, newState.GetPiece(piece.id));
        // If actions didn't work, then return true
        if (!actionWorked)
        {
            newState.QueueFree();
            return true;
        }
        newState.NextTurn();
        
        // Check if the player is still in check
        CheckType result = newState.playerCheck[piece.teamId];
        newState.QueueFree();
        if (result == CheckType.NONE)
        {
            return false;
        }
        return true;
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

    private bool DoActionsAt(Vector2I actionLocation, Piece piece)
    {
        bool didAct = false;
        // Loop through all actions, and find the ones at x, y.
        foreach (ActionBase action in piece.currentPossibleActions)
        {
            if (action.actionLocation == actionLocation)
            {
                // GD.Print($"{(tempState ? "(temp) " : "")}Doing action {action.GetType().Name} with {piece.info.pieceId}:{piece.id}");
                didAct |= TakeAction(action, piece);
            }
        }
        return didAct;
    }

    public void TakeActionAt(Vector2I actionLocation, Piece piece)
    {
        // Get the possible actions for this piece
        if (piece.currentPossibleActions.Count == 0)
        {
            CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.SendNotice, currentPlayerNum, "No actions available at selected location.");
            CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.ActionProcessed, false, actionLocation, piece);
            return;
        }
        
        // If the player is in check, make sure the actions are valid
        if (DoesActionCheck(actionLocation, piece))
        {
            CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.SendNotice, currentPlayerNum, "Action leads to Check.");
            CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.ActionProcessed, false, actionLocation, piece);
            return;
        }

        CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.ActionProcessed, DoActionsAt(actionLocation, piece), actionLocation, piece);
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
        
        // Reset player check
        for (int i = 0; i < playerCheck.Length; i++)
        {
            playerCheck[i] = CheckType.NONE;
        }

        // Loop again, to disable certain check moves
        bool[] hasKing = new bool[GameController.NUMBER_OF_PLAYERS];
        for (int i = 0; i < GameController.NUMBER_OF_PLAYERS; i++)
        {
            hasKing[i] = false;
        }
        
        foreach (Piece piece in allPieces)
        {
            // If it's the king, disable any attacks on it and stop it from moving into
            // a space with check
            if (piece.info.pieceId != KingId)
            {
                continue;
            }

            hasKing[piece.teamId] = true;
            
            if (actionGrid.TryGetCellAt(piece.cell.x, piece.cell.y, out GridCell<ActionBase> cell))
            {
                foreach (GridItem<ActionBase> item in cell.items)
                {
                    if (item is AttackAction)
                    {
                        AttackAction attackAction = (AttackAction)item;
                        // If attack is valid, and is able to check, then mark it down for the player
                        if (playerCheck[piece.teamId] == CheckType.NONE)
                        {
                            if (attackAction.owner.teamId != piece.teamId)
                            {
                                if (!attackAction.verifyTags.Contains("no_check"))
                                {
                                    playerCheck[piece.teamId] = CheckType.IN_CHECK;
                                }
                            }
                        }
                        
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

            // Check for a single valid move action. If there is none, and the piece is in
            // check, then it's a possible checkmate
            bool canMove = false;
            foreach (ActionBase action in piece.currentPossibleActions)
            {
                if (action is MoveAction moveAction)
                {
                    if (moveAction.valid && moveAction.acting)
                    {
                        canMove = true;
                        break;
                    }
                }
            }

            if (!canMove && playerCheck[piece.teamId] == CheckType.IN_CHECK)
            {
                playerCheck[piece.teamId] = CheckType.POSSIBLE_CHECKMATE;
            }
        }
        
        // If no king, put player into check
        // This prevents players playing and losing their king
        for (int teamNum = 0 ; teamNum < GameController.NUMBER_OF_PLAYERS; teamNum++)
        {
            if (!hasKing[teamNum])
            {
                if (playerCheck[teamNum] == CheckType.NONE)
                {
                    playerCheck[teamNum] = CheckType.POSSIBLE_CHECKMATE;
                }
            }
        }

        if (tempState)
        {
            return;
        }
        // If it's non-check, ignore as server already did the check
        if (!needToCheck)
        {
            return;
        }
        
        // Check if each player is in check
        bool checkmate = false;
        for (int teamNum = 0; teamNum < GameController.NUMBER_OF_PLAYERS; teamNum++)
        {
            // Ignore if not in check
            if (playerCheck[teamNum] == CheckType.NONE)
            {
                continue;
            }
            
            // If the team is not the one playing, it means they won't be able to move anyway
            if (teamNum != currentPlayerNum)
            {
                continue;
            }
            
            // If they are playing, then check if they're in check or checkmate
            if (playerCheck[teamNum] == CheckType.IN_CHECK)
            {
                // Check can be ignored, as it means the King, at least, can move out of Check
                continue;
            }
            
            CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.SendNotice, -1, "Checking for Checkmate.");
            
            // If the King is possibly in Checkmate, each possible action needs to be checked.
            bool foundNoCheck = false;
            foreach (var piece in allPieces)
            {
                // Only check for the current team being checked
                if (piece.teamId != teamNum)
                {
                    continue;
                }
                // For each piece, we need to check if the piece can make any action that results
                // in no check. If there is a SINGLE action, then we can stop checking. Otherwise, we continue.
                // In the instance that the player is in checkmate, they are out of the game.
                HashSet<Vector2I> checkedLocations = new HashSet<Vector2I>();
                foreach (var action in piece.currentPossibleActions)
                {
                    // If action is invalid, then ignore it
                    if (!action.valid || !action.acting)
                    {
                        continue;
                    }
                    // If action location is already done, ignore
                    if (checkedLocations.Contains(action.actionLocation))
                    {
                        continue;
                    }
                    checkedLocations.Add(action.actionLocation);
                    if (!DoesActionCheck(action.actionLocation, piece))
                    {
                        GD.Print($"Found no check with {piece.info.pieceId}:{piece.id} with action ({action.actionLocation.X}, {action.actionLocation.Y})");
                        foundNoCheck = true;
                        break;
                    }
                }

                if (foundNoCheck)
                {
                    break;
                }
            }

            // If no check was found, then the player has lost.
            checkmate = !foundNoCheck;
            if (foundNoCheck)
            {
                CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.SendNotice, -1, "Not Checkmate.");
            }
        }

        if (checkmate)
        {
            CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.SendNotice, -1, "Checkmate!");
            CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.PlayerLost, currentPlayerNum);
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
        CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.EndTurn);
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
        CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.NewTurn, currentPlayerNum);
    }

    public void NextTurn()
    {
        NextTurn(-1);
    }

    

    public object Clone()
    {
        // Initialise the new state
        GameState newState = new GameState(gameController);
        newState.Init(needToCheck);

        // Copy over the pieces and their actions
        foreach (var piece in allPieces)
        {
            Piece newPiece = (Piece)piece.Clone();
            newState.allPieces.Add(newPiece);
            newState.grid.PlaceItemAt(newPiece, piece.cell.x, piece.cell.y);
        }
        
        // With all the new pieces, Clone the actions
        foreach (var newPiece in newState.allPieces)
        {
            Piece piece = GetPiece(newPiece.id);
            System.Collections.Generic.Dictionary<int, ActionBase> actions = new System.Collections.Generic.Dictionary<int, ActionBase>();
            System.Collections.Generic.Dictionary<int, List<int>> actionDependents = new System.Collections.Generic.Dictionary<int, List<int>>();
            System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, int>> extraLinks = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, int>>();
            foreach (var action in piece.currentPossibleActions)
            {
                ActionBase newAction = (ActionBase)action.Clone();
                newPiece.AddAction(newAction);
                newAction.SetOwner(newPiece);
                newState.actionGrid.PlaceItemAt(newAction, newAction.actionLocation.X, newAction.actionLocation.Y);
                actions.Add(newAction.actionId, newAction);
                // Get all the dependencies and get their ids.
                List<int> dependents = new List<int>();
                foreach (var dependent in action.dependents)
                {
                    dependents.Add(dependent.actionId);
                }
                actionDependents.Add(newAction.actionId, dependents);
                
                System.Collections.Generic.Dictionary<string, int> links = action.GetExtraCopyLinks();
                /*if (links != null)
                {
                    string text = "{ ";
                    foreach (var keyValue in links)
                    {
                        text += $"({keyValue.Key} :: {keyValue.Value}), ";
                    }

                    GD.Print($"{text} }}");
                }*/

                if (links != null && links.Count > 0)
                {
                    extraLinks.Add(newAction.actionId, links);
                }
            }
            
            // After actions are copied, create dependent links
            foreach (var action in newPiece.currentPossibleActions)
            {
                if (actionDependents.TryGetValue(action.actionId, out List<int> dependents))
                {
                    foreach (var dependentId in dependents)
                    {
                        if (actions.TryGetValue(dependentId, out ActionBase dependent))
                        {
                            action.AddDependent(dependent);
                        }
                        else
                        {
                            GD.Print($"Missing dependent: {dependentId}");
                        }
                    }
                }
                
                // Also give it the information for extra links
                if (extraLinks.TryGetValue(action.actionId, out System.Collections.Generic.Dictionary<string, int> actionLinks))
                {
                    action.SetExtraCopyLinks(newState, actionLinks, actions);
                }
            }
        }
        
        // Copy over variables
        newState.currentPlayerNum = currentPlayerNum;
        newState.gridSize = gridSize;
        newState.lastId = lastId;

        return newState;
    }
}