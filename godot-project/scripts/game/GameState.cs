using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class GameState : Node
{
    private GameController gameController;
    
    public Grid<Piece> pieceGrid;
    public Grid<ActionBase> actionGrid;
    
    private int lastId = 0;
    public int currentPlayerNum;

    public int numberOfPlayers { get; private set; }
    public string KingId { get; set; } = "king";

    private Vector2I _gridUpperCorner;
    public Vector2I gridUpperCorner
    {
        get { return _gridUpperCorner; }
        set
        {
            _gridUpperCorner = value;
            CallDeferred(MethodName.EmitSignal, SignalName.UpperBoundChanged, value);
        }
    }
    
    private Vector2I _gridLowerCorner;

    public Vector2I gridLowerCorner
    {
        get { return _gridLowerCorner; }
        set
        {
            _gridLowerCorner = value;
            CallDeferred(MethodName.EmitSignal, SignalName.LowerBoundChanged, value);
        }
    }

    public Array<Piece> allPieces;
    
    public RandomNumberGenerator gameRandom { get; private set; }
    
    bool tempState;
    private bool tempActionGrid;
    
    // If the State is on the server. Allows game to avoid checking Checkmate if it's on
    // a Client side, given the Client can be told by the server.
    bool isServer = true;

    public GameEvents gameEvents;
    public Array<CardBase> cards;
    
    // Game variables for cards
    public Piece lastMovePiece { get; internal set; }
    public Piece lastTakenPiece { get; private set; }
    public Piece lastAttackerPiece { get; private set; }

    public enum CheckType
    {
        None,
        NoKing,
        InCheck,
        PossibleCheckmate,
        PossibleStalemate
    }

    public CheckType[] playerCheck;

    public GameState(GameController gameController, int numberOfPlayers)
    {
        this.gameController = gameController;
        this.numberOfPlayers = numberOfPlayers;
    }

    internal void Init(bool needToCheck)
    {
        lastId = 0;
        pieceGrid = new Grid<Piece>();
        CallDeferred(Node.MethodName.AddChild, pieceGrid);
        actionGrid = new Grid<ActionBase>();
        CallDeferred(Node.MethodName.AddChild, actionGrid);
        allPieces = new Array<Piece>();
        gridUpperCorner = new Vector2I(7, 7);
        gridLowerCorner = new Vector2I(0, 0);

        gameEvents = new GameEvents(this);
        gameRandom = new RandomNumberGenerator();
        cards = new Array<CardBase>();

        playerCheck = new CheckType[numberOfPlayers];
        for (int i = 0; i < playerCheck.Length; i++)
        {
            playerCheck[i] = CheckType.None;
        }

        isServer = needToCheck;
    }
    
    public PieceDirection GetTeamDirection(int teamId)
    {
        switch (teamId)
        {
            case 0:
                return PieceDirection.Up;
            case 1:
                return PieceDirection.Down;
        }
        return PieceDirection.None;
    }
    
    public void SetPlayerNum(int newPlayerNum)
    {
        currentPlayerNum = newPlayerNum;
        currentPlayerNum %= numberOfPlayers;
        if (currentPlayerNum < 0)
        {
            currentPlayerNum = 0;
        }
    }

    public PieceInfo GetPieceInfo(string pieceInfoId)
    {
        return gameController.GetPieceInfo(pieceInfoId);
    }

    public bool TryGetPieceInfo(string pieceInfoId, out PieceInfo info)
    {
        return gameController.TryGetPieceInfo(pieceInfoId, out info);
    }
    
    
    public Piece PlacePiece(string pieceInfoId, int linkId, int teamId, int x, int y, int id = -1)
    {
        PieceInfo info = gameController.pieceInfoRegistry.GetValue(pieceInfoId);
        if (info == null)
        {
            GD.PushWarning("Tried to place a piece with {pieceId}, even though it hasn't been registered!");
            // return null;
        }

        
        Piece newPiece = new Piece();
        allPieces.Add(newPiece);
        newPiece.SetInfoWithoutSignal(info);
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

        newPiece.forwardDirection = GetTeamDirection(teamId);

        pieceGrid.PlaceItemAt(newPiece, x, y);

        return newPiece;
    }

    public void PutPiece(Piece piece, int x, int y)
    {
        pieceGrid.PlaceItemAt(piece, x, y);
    }
    
    public void PutPiece(int pieceId, int x, int y)
    {
        if (TryGetPiece(pieceId, out Piece piece))
        {
            pieceGrid.PlaceItemAt(piece, x, y);
            return;
        }
        GD.PushError($"Unable to find Piece of id {pieceId} to put.");
    }

    public void MovePiece(Piece piece, int x, int y)
    {
        lastMovePiece = piece;
        pieceGrid.PlaceItemAt(piece, x, y);
    }

    public void MovePiece(int pieceId, int x, int y)
    {
        if (TryGetPiece(pieceId, out Piece piece))
        {
            MovePiece(piece, x, y);
            return;
        }
        GD.PushError($"Unable to find Piece of id {pieceId} to move.");
    }
    
    public bool TakePiece(Piece piece, Piece attacker = null)
    {
        if (piece == null || !allPieces.Contains(piece))
        {
            return false;
        }
        // Remove it from the board
        pieceGrid.RemoveItem(piece);
        
        // Move to takenPieces
        allPieces.Remove(piece);
        
        // Emit signal
        CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.PieceRemoved, piece, attacker);
        
        // Free the item. This also frees the action data
        piece.CallDeferred(Node.MethodName.QueueFree);
        
        // Remove the actions from the grid
        foreach (var action in piece.currentPossibleActions)
        {
            if (piece == action.owner)
            {
                action.cell.RemoveItem(action);
                action.CallDeferred(Node.MethodName.QueueFree);
            }
        }

        lastTakenPiece = piece;
        lastAttackerPiece = attacker;
        
        gameEvents.AnnounceEvent(GameEvents.PieceTaken);
        
        return true;
    }
    
    public bool TakePiece(int pieceId, int attackerId = -1)
    {
        return TakePiece(GetPiece(pieceId), GetPiece(attackerId));
    }

    public Piece GetFirstPieceAt(int x, int y)
    {
        if (pieceGrid.TryGetCellAt(x, y, out GridCell<Piece> cell)) {
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
        if (pieceGrid.TryGetCellAt(x, y, out GridCell<Piece> cell))
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
        if (pieceGrid.TryGetCellAt(x, y, out GridCell<Piece> cell))
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
        if (pieceGrid.TryGetCellAt(x, y, out GridCell<Piece> cell))
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

    public bool IsPieceAtEndOfBound(Piece piece)
    {
        // Depending on forward direction, choose comparison
        switch (piece.forwardDirection)
        {
            case PieceDirection.Down:
                return piece.cell.y == gridLowerCorner.Y;
            case PieceDirection.Up:
                return piece.cell.y == gridUpperCorner.Y;
            case PieceDirection.Left:
                return piece.cell.x == gridLowerCorner.X;
            case PieceDirection.Right:
                return piece.cell.x == gridUpperCorner.X;
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
            // GD.Print($"Invalid {action.GetType().Name} ({action.actionLocation.X}, {action.actionLocation.Y}): Action is not acting.");
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
            return PlayerInCheck(piece.teamId);
        }
        // If it's non-check, ignore as server already did the check
        if (!isServer)
        {
            return false;
        }

        StoreCurrentValidation();
        // Simulate the movement, and check if the player is still in check
        GameState newState = Clone();
        CallDeferred(Node.MethodName.AddChild, newState);
        newState.tempState = true;
        newState.currentPlayerNum = piece.teamId;
        // Temporarily use the existing action grid so that it doesn't have to be cloned.
        newState.actionGrid = actionGrid;
        newState.tempActionGrid = true;
        
        // Do the actions, and go to the next turn
        bool actionWorked = newState.DoActionsAt(actionLocation, newState.GetPiece(piece.id));
        // If actions didn't work, then return if it's in check or not
        if (!actionWorked)
        {
            newState.CallDeferred(Node.MethodName.QueueFree);
            RestoreValidation();
            return PlayerInCheck(piece.teamId);
        }
        newState.NextTurn();
        
        // Check if the player is still in check
        bool playerInCheck = newState.PlayerInCheck(piece.teamId);
        newState.CallDeferred(Node.MethodName.QueueFree);
        RestoreValidation();
        return playerInCheck;
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
        if (piece == null)
        {
            return false;
        }
        bool didAct = false;
        // Loop through all actions, and find the ones at x, y.
        if (actionGrid.TryGetCellAt(actionLocation, out GridCell<ActionBase> actionCell))
        {
            foreach (var actionGridItem in actionCell.items)
            {
                ActionBase action = (ActionBase)actionGridItem;
                if (action.owner.id == piece.id)
                {
                    didAct |= TakeAction(action, piece);
                }
            }
        }
        else
        {
            GD.Print($"Tried to make Piece {piece.id} take Actions at {actionLocation}, but the GridCell doesn't exist.");
        }
        return didAct;
    }

    public bool TakeActionAt(Vector2I actionLocation, Piece piece)
    {
        // If piece is null, fail
        if (piece is null)
        {
            return false;
        }
        // Get the possible actions for this piece
        if (piece.currentPossibleActions == null || piece.currentPossibleActions.Count == 0)
        {
            CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.SendNotice, currentPlayerNum, "No actions available at selected location.");
            CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.ActionProcessed, false, actionLocation, piece);
            return false;
        }
        
        // If the player is in check, make sure the actions are valid
        if (DoesActionCheck(actionLocation, piece))
        {
            CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.SendNotice, currentPlayerNum, "Action leads to Check.");
            CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.ActionProcessed, false, actionLocation, piece);
            return false;
        }

        bool returnVal = DoActionsAt(actionLocation, piece);
        CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.ActionProcessed, returnVal, actionLocation, piece);
        return returnVal;
    }
    
    

    public void StartGame()
    {
        gameEvents.AnnounceEvent(GameEvents.StartGame);
        // Tell all pieces to update their possible moves
        PiecesNewTurn();
        gameEvents.AnnounceEvent(GameEvents.GameStarted);
    }

    public bool PlayerInCheck(int playerNum)
    {
        if (playerNum < 0 || playerNum >= playerCheck.Length)
        {
            GD.PushError($"Tried to check if player {playerNum + 1} is in check, when there are only {playerCheck.Length} players (0 - {playerCheck.Length - 1}.");
            return false;
        }

        return playerCheck[playerNum] == CheckType.InCheck || playerCheck[playerNum] == CheckType.PossibleCheckmate;
    }

    public bool PlayerHasNoKing(int playerNum)
    {if (playerNum < 0 || playerNum >= playerCheck.Length)
        {
            GD.PushError($"Tried to check if player {playerNum + 1} has no King, when there are only {playerCheck.Length} players (0 - {playerCheck.Length - 1}.");
            return false;
        }

        return playerCheck[playerNum] == CheckType.NoKing;
    }

    private void StoreCurrentValidation()
    {
        foreach (var piece in allPieces)
        {
            foreach (var action in piece.currentPossibleActions)
            {
                action.StoredValidation = action.valid;
            }
        }
    }

    private void RestoreValidation()
    {
        foreach (var piece in allPieces)
        {
            foreach (var action in piece.currentPossibleActions)
            {
                action.SetValidToStored();
            }
        }
    }
    
    private void PiecesNewTurn()
    {
        // Announce that a new turn is about to start
        gameEvents.AnnounceEvent(GameEvents.PreNewTurn);
        foreach (var piece in allPieces)
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
            playerCheck[i] = CheckType.None;
        }

        // Loop again, to disable certain check moves
        int[] kingCount = new int[numberOfPlayers];
        
        List<Piece> kings = new List<Piece>();
        
        // Find all the kings
        foreach (var piece in allPieces)
        {
            if (piece.info == null || piece.info.pieceId != KingId)
            {
                continue;
            }
            kings.Add(piece);
            kingCount[piece.teamId] += 1;
        }
        
        // Loop through kings to check for Check
        foreach (var king in kings)
        {
            // If there are too many kings for its team, ignore
            if (kingCount[king.teamId] > 1)
            {
                continue;
            }
            // If it's the king, disable any attacks on it and stop it from moving into
            // a space with check
            if (actionGrid.TryGetCellAt(king.cell.x, king.cell.y, out GridCell<ActionBase> cell))
            {
                foreach (GridItem<ActionBase> item in cell.items)
                {
                    if (item is AttackAction attackAction)
                    {
                        // If invalid, or not acting, ignore the attack
                        if (!attackAction.valid || !attackAction.acting)
                        {
                            continue;
                        }
                        // If attack is valid, and is able to check, then mark it down for the player
                        if (playerCheck[king.teamId] == CheckType.None)
                        {
                            if (attackAction.owner.teamId != king.teamId)
                            {
                                if (!attackAction.verifyTags.Contains("no_check"))
                                {
                                    playerCheck[king.teamId] = CheckType.InCheck;
                                }
                            }
                        }
                        
                        // Once done, make the attack action, and a move action if it has it, invalid
                        // as the king can't be attacked.
                        attackAction.MakeInvalid();
                        if (attackAction.moveAction != null)
                        {
                            attackAction.moveAction.MakeInvalid();
                        }
                    }
                }
            }
        }
        
        // Stop temp states here as they don't need to do the following checks
        if (tempState)
        {
            return;
        }
        
        // Go through the Kings again, and disable moves that would move into
        // check.
        foreach (var king in kings)
        {
            // If there are too many kings for its team, ignore
            if (kingCount[king.teamId] > 1)
            {
                continue;
            }

            System.Collections.Generic.Dictionary<Vector2I, List<ActionBase>> locationToAction = new();
            foreach (var action in king.currentPossibleActions)
            {
                // If the action isn't valid, ignore
                if (!action.valid)
                {
                    continue;
                }
                if (!locationToAction.TryGetValue(action.actionLocation, out List<ActionBase> actions))
                {
                    actions = new List<ActionBase>();
                    locationToAction.Add(action.actionLocation, actions);
                }

                actions.Add(action);
            }

            bool canMove = false;
            foreach (var location in locationToAction)
            {
                if (DoesActionCheck(location.Key, king))
                {
                    // Disable all actions at this location if it does check the King, but
                    // only the actions at this location.
                    foreach (var action in location.Value)
                    {
                        action.MakeInvalid(ActionBase.CarryType.None);
                    }
                }
                else
                {
                    canMove = true;
                }
            }

            if (!canMove)
            {
                if (playerCheck[king.teamId] == CheckType.InCheck)
                {
                    playerCheck[king.teamId] = CheckType.PossibleCheckmate;
                }
                else
                {
                    playerCheck[king.teamId] = CheckType.PossibleStalemate;
                }
            }
        }
        
        // If it's non-check, ignore as server already did the check
        if (!isServer)
        {
            return;
        }
        
        // Check if either player is missing a King
        // This is prioritised over being in Checkmate, as having no King means Checkmate
        // isn't possible
        for (int teamNum = 0; teamNum < numberOfPlayers; teamNum++)
        {
            if (kingCount[teamNum] == 0)
            {
                // Signal that the player has lost.
                playerCheck[teamNum] = CheckType.NoKing; // Put player as having No King, so that checkmate isn't checked for
                CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.SendNotice, -1, $"With no {StringUtil.ToTitleCase(KingId)}, Player {teamNum+1}'s army is lost.");
                CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.PlayerLost, teamNum);
            }
        }
        
        // Check if each player is in check
        bool checkmate = false;
        for (int teamNum = 0; teamNum < numberOfPlayers; teamNum++)
        {
            // If the team is not the one playing, it means they won't be able to move anyway
            if (teamNum != currentPlayerNum)
            {
                continue;
            }

            // No need to check for King in Checkmate if there is no King
            if (kingCount[teamNum] == 0)
            {
                continue;
            }
            
            // If they are playing, then check if they're in check or checkmate
            if (playerCheck[teamNum] == CheckType.None)
            {
                // Check can be ignored, as it means the King, at least, can move out of Check
                break;
            }
            
            // If King can move, they have an action they can take
            // TODO: Certain cards may change this fact. King movement check will need to take Cards into account by
            // checking if MoveAction checks, rather than if the King is in Checkmate.
            if (playerCheck[teamNum] != CheckType.PossibleCheckmate)
            {
                break;
            }
            
            CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.SendNotice, -1, "Checking for Checkmate.");
            
            // If the King is possibly in Checkmate, each possible action needs to be checked.
            bool foundNoCheck = false;
            foreach (var piece in allPieces)
            {
                // Only check for the current team being checked, and if the piece has info
                if (piece.info == null || piece.teamId != teamNum)
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
                    if (!checkedLocations.Add(action.actionLocation))
                    {
                        continue;
                    }
                    if (!DoesActionCheck(action.actionLocation, piece))
                    {
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
        
        // Check if the game is in Stalemate
        // Check if the current playing team is possibly in stalemate.
        if (playerCheck[currentPlayerNum] == CheckType.PossibleStalemate)
        {
            bool isStalemate = true;
            // Check all piece actions
            foreach (var piece in allPieces)
            {
                // Only check the current team
                if (piece.teamId != currentPlayerNum)
                {
                    continue;
                }

                // If there are any valid actions, then the game is not in stalemate.
                List<Vector2I> checkLocations = new();
                foreach (var action in piece.currentPossibleActions)
                {
                    // If the location is already checked, or the action is invalid, ignore
                    if (checkLocations.Contains(action.actionLocation) || !action.valid)
                    {
                        continue;
                    }
                    checkLocations.Add(action.actionLocation);
                    // Only continue if the move does not result in Check. This is neeeded,
                    // otherwise it will accept moves that aren't possible due to them putting
                    // the player in to Check.
                    if (!DoesActionCheck(action.actionLocation, piece))
                    {
                        isStalemate = false;
                        break;
                    }
                }

                // If it's not stalemate, break out of the piece loop.
                if (!isStalemate)
                {
                    break;
                }
            }
            
            // If isStalemate is still true, the game is over.
            if (isStalemate)
            {
                CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.SendNotice, -1, "Stalemate.");
                CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.GameStalemate, currentPlayerNum);
            }
        }
    }

    public void NextTurn(int newPlayerNum)
    {
        // If action grid was temporary, replace it with a new one
        if (tempActionGrid)
        {
            Grid<ActionBase> oldActions = actionGrid;
            actionGrid = new Grid<ActionBase>();
            CallDeferred(Node.MethodName.AddChild, actionGrid);
            
            // Re-add old actions if their piece doesn't need updating
            foreach (var cell in oldActions.cells)
            {
                foreach (var item in cell.items)
                {
                    ActionBase action = (ActionBase)item;
                    if (TryGetPiece(action.owner.id, out Piece piece))
                    {
                        if (!piece.needsActionUpdate)
                        {
                            actionGrid.PlaceItemAt(action, action.actionLocation.X, action.actionLocation.Y, false);
                        }
                    }
                }
            }
        }
        // First, end turn
        gameEvents.AnnounceEvent(GameEvents.EndTurn);
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
                    if (action.cell != null && action.owner == piece)
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
        gameEvents.AnnounceEvent(GameEvents.NewTurn);
        CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.NewTurn, currentPlayerNum);
    }

    // Alternate function (for Godot to call)
    public void NextTurn()
    {
        NextTurn(-1);
    }


    internal void AddCard(CardBase card, bool callCardAdd)
    {
        cards.Add(card);
        card.MakeListeners(gameEvents);
        if (callCardAdd)
        {
            card.OnAddCard(this);
        }
        CallDeferred(Node.MethodName.AddChild, card);
    }

    public List<CardBase> GetExistingCards(string cardId)
    {
        List<CardBase> existingCards = new List<CardBase>();
        foreach (var card in cards)
        {
            if (card.cardId == cardId)
            {
                existingCards.Add(card);
            }
        }

        return existingCards;
    }

    public void AddVerificationRule(string ruleId, string pieceId = null)
    {
        if (!gameController.validationRuleRegistry.TryGetValue(ruleId, out ValidationRuleBase rule))
        {
            GD.PushError($"Tried to add a verification rule {ruleId} when it has not been registered.");
            return;
        }
        
        // If target Piece is defined, add directly to the PieceInfo
        if (pieceId != null)
        {
            if (gameController.pieceInfoRegistry.TryGetValue(pieceId, out PieceInfo info))
            {
                info.AddValidationRule(rule);
            }
            // If it didn't get the info, error
            GD.PushError($"Couldn't get the PieceInfo for piece id {pieceId}.");
            return;
        }
        
        // If no pieceId is defined, add it to all pieceIds
        foreach (var info in gameController.pieceInfoRegistry.GetValues())
        {
            info.AddValidationRule(rule);
        }
    }




    public string[] GetAllPieceIds()
    {
        return gameController.pieceInfoRegistry.GetKeys();
    }
    
    
    
    public GameState Clone()
    {
        // Initialise the new state
        GameState newState = new GameState(gameController, numberOfPlayers);
        newState.Init(isServer);
        // Copy over the seed so any randomness follows the same
        newState.gameRandom.Seed = gameRandom.Seed;
        
        // Copy the Cards
        foreach (var card in cards)
        {
            newState.AddCard(card.Clone(), false);
        }

        // Copy over the pieces
        foreach (var piece in allPieces)
        {
            Piece newPiece = piece.Clone();
            newState.allPieces.Add(newPiece);
            newState.pieceGrid.PlaceItemAt(newPiece, piece.cell.x, piece.cell.y);
            // Add all the items' actions
            foreach (var action in piece.currentPossibleActions)
            {
                newPiece.AddAction(action);
            }
        }
        
        // Copy over variables
        newState.currentPlayerNum = currentPlayerNum;
        newState.gridUpperCorner = gridUpperCorner;
        newState.gridLowerCorner = gridLowerCorner;
        newState.lastId = lastId;
        newState.KingId = KingId;
        
        return newState;
    }
}