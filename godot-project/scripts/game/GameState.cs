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

    public string KingId { get; internal set; } = "king";

    public Vector2I gridSize;

    public Array<Piece> allPieces;
    
    public RandomNumberGenerator gameRandom { get; private set; }
    
    bool tempState;
    
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
        PossibleCheckmate
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

        gameEvents = new GameEvents(this);
        gameRandom = new RandomNumberGenerator();
        cards = new Array<CardBase>();

        playerCheck = new CheckType[GameController.NUMBER_OF_PLAYERS];
        for (int i = 0; i < playerCheck.Length; i++)
        {
            playerCheck[i] = CheckType.None;
        }

        this.isServer = needToCheck;
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

    public PieceInfo GetPieceInfo(string pieceId)
    {
        return gameController.GetPieceInfo(pieceId);
    }

    public bool TryGetPieceInfo(string pieceId, out PieceInfo info)
    {
        return gameController.TryGetPieceInfo(pieceId, out info);
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

    public void MovePiece(Piece piece, int x, int y)
    {
        lastMovePiece = piece;
        grid.PlaceItemAt(piece, x, y);
        gameEvents.AnnounceEvent(GameEvents.PieceMoved);
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

        lastTakenPiece = piece;
        lastAttackerPiece = attacker;
        
        gameEvents.AnnounceEvent(GameEvents.PieceTaken);
        
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
            return PlayerInCheck(piece.teamId);
        }
        // If it's non-check, ignore as server already did the check
        if (!isServer)
        {
            return false;
        }
        // Simulate the movement, and check if the player is still in check
        GameState newState = (GameState)Clone();
        CallDeferred(Node.MethodName.AddChild, newState);
        newState.tempState = true;
        
        // Do the actions, and go to the next turn
        bool actionWorked = newState.DoActionsAt(actionLocation, newState.GetPiece(piece.id));
        // If actions didn't work, then return if it's in check or not
        if (!actionWorked)
        {
            newState.QueueFree();
            return PlayerInCheck(piece.teamId);
        }
        newState.NextTurn();
        
        // Check if the player is still in check
        bool playerInCheck = newState.PlayerInCheck(piece.teamId);
        newState.QueueFree();
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

    public bool TakeActionAt(Vector2I actionLocation, Piece piece)
    {
        // Get the possible actions for this piece
        if (piece.currentPossibleActions.Count == 0)
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
        // Tell all pieces to update their possible moves
        PiecesNewTurn();
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
            playerCheck[i] = CheckType.None;
        }

        // Loop again, to disable certain check moves
        bool[] hasKing = new bool[GameController.NUMBER_OF_PLAYERS];
        for (int i = 0; i < GameController.NUMBER_OF_PLAYERS; i++)
        {
            hasKing[i] = false;
        }
        
        List<Piece> kings = new List<Piece>();
        
        foreach (Piece piece in allPieces)
        {
            // If it's the king, disable any attacks on it and stop it from moving into
            // a space with check
            if (piece.info.pieceId != KingId)
            {
                continue;
            }
            
            kings.Add(piece);
            
            hasKing[piece.teamId] = true;
            
            if (actionGrid.TryGetCellAt(piece.cell.x, piece.cell.y, out GridCell<ActionBase> cell))
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
                        if (playerCheck[piece.teamId] == CheckType.None)
                        {
                            if (attackAction.owner.teamId != piece.teamId)
                            {
                                if (!attackAction.verifyTags.Contains("no_check"))
                                {
                                    playerCheck[piece.teamId] = CheckType.InCheck;
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
            // TODO: Change Kings to check all possible actions to see if they're possible (with DoesActionCheck)
            // This will only check the possible actions for a single piece, so it shouldn't take
            // too much time to do, while also improving the functionality.
            foreach (ActionBase action in king.currentPossibleActions)
            {
                // If action is invalid, ignore
                if (!action.valid)
                {
                    continue;
                }
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
                    // If action is not acting, ignore
                    // invalid actions are not ignored as, for example, bishops movement would be
                    // considered "invalid" beyond the king, but the king can't move onto these spaces as they
                    // are in check
                    if (!attackAction.acting)
                    {
                        continue;
                    }

                    // Ignore actions that can't check
                    if (attackAction.verifyTags.Contains("no_check"))
                    {
                        continue;
                    }

                    if (attackAction.owner.teamId != king.teamId)
                    {
                        moveAction.MakeInvalid();
                    }
                }
            }

            // Check for a single valid move action. If there is none, and the piece is in
            // check, then it's a possible checkmate
            bool canMove = false;
            foreach (ActionBase action in king.currentPossibleActions)
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

            if (!canMove && playerCheck[king.teamId] == CheckType.InCheck)
            {
                playerCheck[king.teamId] = CheckType.PossibleCheckmate;
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
        for (int teamNum = 0; teamNum < GameController.NUMBER_OF_PLAYERS; teamNum++)
        {
            if (!hasKing[teamNum])
            {
                // Signal that the player has lost.
                playerCheck[teamNum] = CheckType.NoKing; // Put player as having No King, so that checkmate isn't checked for
                CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.SendNotice, -1, $"With no {StringUtil.ToTitleCase(KingId)}, Player {teamNum+1}'s army is lost.");
                CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.PlayerLost, teamNum);
            }
        }
        
        // Check if each player is in check
        bool checkmate = false;
        for (int teamNum = 0; teamNum < GameController.NUMBER_OF_PLAYERS; teamNum++)
        {
            // If the team is not the one playing, it means they won't be able to move anyway
            if (teamNum != currentPlayerNum)
            {
                continue;
            }
            
            // If they are playing, then check if they're in check or checkmate
            if (playerCheck[teamNum] != CheckType.None)
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
                    if (!checkedLocations.Add(action.actionLocation))
                    {
                        continue;
                    }
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
        gameEvents.AnnounceEvent(GameEvents.NewTurn);
        CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.NewTurn, currentPlayerNum);
    }

    public void NextTurn()
    {
        NextTurn(-1);
    }


    internal void AddCard(CardBase card)
    {
        cards.Add(card);
        card.MakeListeners(gameEvents);
        CallDeferred(Node.MethodName.AddChild, card);
    }
    
    
    
    public GameState Clone()
    {
        // Initialise the new state
        GameState newState = new GameState(gameController);
        newState.Init(isServer);
        // Copy over the seed so any randomness follows the same
        newState.gameRandom.Seed = gameRandom.Seed;
        
        // Copy the Cards
        foreach (var card in cards)
        {
            newState.AddCard(card.Clone());
        }

        // Copy over the pieces
        foreach (var piece in allPieces)
        {
            Piece newPiece = piece.Clone();
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