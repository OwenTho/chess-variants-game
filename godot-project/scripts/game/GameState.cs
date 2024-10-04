using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public partial class GameState : Node {
  public enum CheckType {
    None,
    NoKing,
    InCheck,
    PossibleCheckmate,
    PossibleStalemate
  }

  public Grid<ActionBase> ActionGrid;

  public Array<Piece> AllPieces;
  public Array<CardBase> Cards;
  public int CurrentPlayerNum;

  public GameEvents GameEvents;


  public Grid<Piece> PieceGrid;

  public CheckType[] PlayerCheck;

  public GameState(GameController gameController, int numberOfPlayers) {
    _gameController = gameController;
    NumberOfPlayers = numberOfPlayers;
  }

  public int NumberOfPlayers { get; private set; }
  public string KingId { get; set; } = "king";

  public Vector2I GridUpperCorner {
    get => _gridUpperCorner;
    set {
      _gridUpperCorner = value;
      CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.UpperBoundChanged, value);
    }
  }

  public Vector2I GridLowerCorner {
    get => _gridLowerCorner;
    set {
      _gridLowerCorner = value;
      CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.LowerBoundChanged, value);
    }
  }

  public RandomNumberGenerator GameRandom { get; private set; }

  // If the State is on the server. Allows game to avoid checking Checkmate if it's on
  // a Client side, given the Client can be told by the server.
  public bool IsServer { get; private set; } = true;

  // Game variables for cards
  public Piece LastMovePiece { get; internal set; }
  public Piece LastTakenPiece { get; private set; }
  public Piece LastAttackerPiece { get; private set; }

  // Private variables
  private GameController _gameController;

  private Vector2I _gridLowerCorner;
  private Vector2I _gridUpperCorner;

  private int _lastId;

  private bool _tempActionGrid;

  private bool _tempState;


  internal void Init(bool stateIsServer) {
    _lastId = 0;
    PieceGrid = new Grid<Piece>();
    CallDeferred(Node.MethodName.AddChild, PieceGrid);
    ActionGrid = new Grid<ActionBase>();
    CallDeferred(Node.MethodName.AddChild, ActionGrid);
    AllPieces = new Array<Piece>();
    GridUpperCorner = new Vector2I(7, 7);
    GridLowerCorner = new Vector2I(0, 0);

    GameEvents = new GameEvents(this, true);
    CallDeferred(Node.MethodName.AddChild, GameEvents);
    GameRandom = new RandomNumberGenerator();
    Cards = new Array<CardBase>();

    PlayerCheck = new CheckType[NumberOfPlayers];
    for (int i = 0; i < PlayerCheck.Length; i++) {
      PlayerCheck[i] = CheckType.None;
    }

    IsServer = stateIsServer;
  }


  // Public Methods

  public PieceDirection GetTeamDirection(int teamId) {
    switch (teamId) {
      case 0:
        return PieceDirection.Up;
      case 1:
        return PieceDirection.Down;
    }

    return PieceDirection.None;
  }

  public void SetPlayerNum(int newPlayerNum) {
    CurrentPlayerNum = newPlayerNum;
    CurrentPlayerNum %= NumberOfPlayers;
    if (CurrentPlayerNum < 0) {
      CurrentPlayerNum = 0;
    }
  }

  public Array<int> GetPlayersInCheck() {
    var playersInCheck = new Array<int>();
    for (int i = 0; i < NumberOfPlayers; i++) {
      if (PlayerInCheck(i)) {
        playersInCheck.Add(i);
      }
    }

    return playersInCheck;
  }

  public PieceInfo GetPieceInfo(string pieceInfoId) {
    return _gameController.GetPieceInfo(pieceInfoId);
  }

  public bool TryGetPieceInfo(string pieceInfoId, out PieceInfo info) {
    return _gameController.TryGetPieceInfo(pieceInfoId, out info);
  }

  public string GetPieceName(string pieceInfoId) {
    return _gameController.GetPieceName(pieceInfoId);
  }

  public PieceInfo[] GetAllPieceInfo() {
    return _gameController.PieceInfoRegistry.GetValues();
  }

  public string GetActionId(ActionBase action) {
    return _gameController.GetActionId(action);
  }

  public ActionRuleBase GetActionRule(string id) {
    return _gameController.GetActionRule(id);
  }

  public Piece PlacePiece(string pieceInfoId, int linkId, int teamId, int x, int y, int id = -1) {
    PieceInfo info = _gameController.PieceInfoRegistry.GetValue(pieceInfoId);
    if (info == null) {
      GD.PushWarning(
        "Tried to place a piece with {pieceId}, even though it hasn't been registered!");
    }
    // return null;

    var newPiece = new Piece();
    newPiece.Taken = false;
    AllPieces.Add(newPiece);
    newPiece.SetInfoWithoutSignal(info);
    if (id != -1) {
      newPiece.Id = id;
    }
    else {
      newPiece.Id = _lastId;
      _lastId += 1;
    }

    newPiece.LinkId = linkId;
    newPiece.TeamId = teamId;

    newPiece.ForwardDirection = GetTeamDirection(teamId);

    PieceGrid.PlaceItemAt(newPiece, x, y);

    return newPiece;
  }

  public void PutPiece(Piece piece, int x, int y) {
    PieceGrid.PlaceItemAt(piece, x, y);
  }

  public void PutPiece(int pieceId, int x, int y) {
    if (TryGetPiece(pieceId, out Piece piece)) {
      PieceGrid.PlaceItemAt(piece, x, y);
      return;
    }

    GD.PushError($"Unable to find Piece of id {pieceId} to put.");
  }

  public void MovePiece(Piece piece, int x, int y) {
    LastMovePiece = piece;
    PieceGrid.PlaceItemAt(piece, x, y);
  }

  public void MovePiece(int pieceId, int x, int y) {
    if (TryGetPiece(pieceId, out Piece piece)) {
      MovePiece(piece, x, y);
      return;
    }

    GD.PushError($"Unable to find Piece of id {pieceId} to move.");
  }

  public bool TakePiece(Piece piece, Piece attacker = null) {
    if (piece == null || !AllPieces.Contains(piece)) {
      return false;
    }

    // Remove it from the board
    PieceGrid.RemoveItem(piece);

    // Move to takenPieces
    AllPieces.Remove(piece);

    // Emit signal
    CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.PieceRemoved, piece, attacker);

    // Mark the piece as taken. This stops all following actions if the Piece is currently taking
    // actions, as the piece is no longer there to take them.
    piece.Taken = true;

    // Free the Piece.
    piece.CallDeferred(Node.MethodName.QueueFree);

    // Remove the actions from the grid, and free them
    foreach (ActionBase action in piece.CurrentPossibleActions) {
      if (piece == action.Owner) {
        action.Cell.RemoveItem(action);
        action.CallDeferred(Node.MethodName.QueueFree);
      }
    }

    LastTakenPiece = piece;
    LastAttackerPiece = attacker;

    GameEvents.AnnounceEvent(GameEvents.PieceTaken);

    return true;
  }

  public bool TakePiece(int pieceId, int attackerId = -1) {
    return TakePiece(GetPiece(pieceId), GetPiece(attackerId));
  }

  public Piece GetFirstPieceAt(int x, int y) {
    if (PieceGrid.TryGetCellAt(x, y, out GridCell<Piece> cell)) {
      foreach (GridItem<Piece> item in cell.Items) {
        if (item is Piece) {
          return (Piece)item;
        }
      }
    }

    return null;
  }

  public bool TryGetFirstPieceAt(int x, int y, out Piece piece) {
    piece = GetFirstPieceAt(x, y);
    return piece != null;
  }

  public Array<Piece> GetPiecesByLinkId(int linkId) {
    var pieces = new Array<Piece>();
    foreach (Piece piece in AllPieces) {
      if (piece.LinkId == linkId) {
        pieces.Add(piece);
      }
    }

    return pieces;
  }


  public bool HasPieceAt(int x, int y) {
    if (PieceGrid.TryGetCellAt(x, y, out GridCell<Piece> cell)) {
      foreach (GridItem<Piece> item in cell.Items) {
        if (item is Piece) {
          return true;
        }
      }
    }

    return false;
  }

  public bool HasPieceIdAt(string pieceId, int x, int y) {
    if (PieceGrid.TryGetCellAt(x, y, out GridCell<Piece> cell)) {
      foreach (GridItem<Piece> item in cell.Items) {
        if (item is Piece) {
          var piece = (Piece)item;
          // If no info, ignore
          if (piece.Info == null) {
            continue;
          }

          // If piece id matches, return true
          if (piece.Info.PieceId == pieceId) {
            return true;
          }
        }
      }
    }

    return false;
  }

  public Piece GetPiece(int pieceId) {
    foreach (Piece piece in AllPieces) {
      if (piece.Id == pieceId) {
        return piece;
      }
    }

    return null;
  }

  public bool TryGetPiece(int pieceId, out Piece piece) {
    piece = GetPiece(pieceId);
    return piece != null;
  }

  public Array<Piece> GetKingPieces() {
    var kingPieces = new Array<Piece>();
    foreach (Piece piece in AllPieces) {
      if (piece.Info != null && piece.Info.PieceId == KingId) {
        kingPieces.Add(piece);
      }
    }

    return kingPieces;
  }


  public Array<Piece> GetPiecesAt(int x, int y) {
    var pieces = new Array<Piece>();
    if (PieceGrid.TryGetCellAt(x, y, out GridCell<Piece> cell)) {
      foreach (GridItem<Piece> item in cell.Items) {
        if (item is Piece) {
          pieces.Add((Piece)item);
        }
      }
    }

    return pieces;
  }

  public bool TryGetPiecesAt(int x, int y, out Array<Piece> pieces) {
    pieces = GetPiecesAt(x, y);
    return pieces.Count > 0;
  }

  public bool IsTargeted(Piece piece) {
    if (ActionGrid.TryGetCellAt(piece.Cell.X, piece.Cell.Y, out GridCell<ActionBase> cell)) {
      foreach (GridItem<ActionBase> item in cell.Items) {
        if (item is AttackAction attackAction)
          // If the attack is from another team, and valid, then return true
        {
          if (piece.TeamId != attackAction.Owner.TeamId) {
            return true;
          }
        }
      }
    }

    return false;
  }

  public bool IsPieceAtEndOfBound(Piece piece) {
    if (piece == null) {
      return false;
    }

    // Depending on forward direction, choose comparison
    switch (piece.ForwardDirection) {
      case PieceDirection.Down:
        return piece.Cell.Y == GridLowerCorner.Y;
      case PieceDirection.Up:
        return piece.Cell.Y == GridUpperCorner.Y;
      case PieceDirection.Left:
        return piece.Cell.X == GridLowerCorner.X;
      case PieceDirection.Right:
        return piece.Cell.X == GridUpperCorner.X;
    }

    return false;
  }

  public bool IsActionValid(ActionBase action, Piece piece) {
    // The piece must be of the current team
    if (piece == null || piece.TeamId != CurrentPlayerNum)
      // GD.Print($"Invalid {action.GetType().Name} ({action.actionLocation.X}, {action.actionLocation.Y}): Piece null or wrong team.");
    {
      return false;
    }

    // Ignore if null
    if (action == null) {
      GD.PushError($"Tried to Act on piece {piece.GetType().Name}, but the Action was null.");
      return false;
    }

    // Ignore if not acting
    if (!action.Acting)
      // GD.Print($"Invalid {action.GetType().Name} ({action.actionLocation.X}, {action.actionLocation.Y}): Action is not acting.");
    {
      return false;
    }

    // Ignore if invalid
    if (!action.Valid)
      // GD.Print($"Invalid {action.GetType().Name} ({action.actionLocation.X}, {action.actionLocation.Y}): Action is invalid: {action.tags}");
    {
      return false;
    }

    return true;
  }

  public bool DoesActionCheck(Vector2I actionLocation, Piece piece) {
    // If it's a temp state, return (to avoid recursion)
    if (_tempState) {
      return PlayerInCheck(piece.TeamId);
    }

    // If it's non-check, ignore as server already did the check
    if (!IsServer) {
      return false;
    }

    StoreCurrentValidation();
    // Simulate the movement, and check if the player is still in check
    GameState newState = Clone();
    CallDeferred(Node.MethodName.AddChild, newState);
    newState._tempState = true;
    newState.GameEvents.CanWait = true; // Can flag wait
    newState.GameEvents.DoWait = false; // But don't wait
    newState.CurrentPlayerNum = piece.TeamId;
    // Temporarily use the existing action grid so that it doesn't have to be cloned.
    newState.ActionGrid = ActionGrid;
    newState._tempActionGrid = true;

    // Do the actions, and go to the next turn
    bool actionWorked = newState.DoActionsAt(actionLocation, newState.GetPiece(piece.Id));
    // If actions didn't work, then return if it's in check or not
    if (!actionWorked) {
      newState.CallDeferred(Node.MethodName.QueueFree);
      RestoreValidation();
      return PlayerInCheck(piece.TeamId);
    }

    newState.NextTurn();

    // Check if the player is still in check
    bool playerInCheck = newState.PlayerInCheck(piece.TeamId);
    newState.CallDeferred(Node.MethodName.QueueFree);
    RestoreValidation();
    return playerInCheck;
  }

  public bool TakeAction(ActionBase action, Piece piece) {
    if (!IsActionValid(action, piece)) {
      return false;
    }

    CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.ActionProcessed, action, piece);
    action.ActOn(this, piece);
    return true;
  }

  public bool TakeActionAt(Vector2I actionLocation, Piece piece) {
    // If piece is null, fail
    if (piece is null) {
      return false;
    }

    // Get the possible actions for this piece
    if (piece.CurrentPossibleActions == null || piece.CurrentPossibleActions.Count == 0) {
      CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.SendNotice, CurrentPlayerNum,
        "No actions available at selected location.");
      CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.ActionsProcessedAt, false,
        actionLocation, piece);
      return false;
    }

    // If the player is in check, make sure the actions are valid
    if (DoesActionCheck(actionLocation, piece)) {
      CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.SendNotice, CurrentPlayerNum,
        "Action leads to Check.");
      CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.ActionsProcessedAt, false,
        actionLocation, piece);
      return false;
    }

    bool returnVal = DoActionsAt(actionLocation, piece);
    CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.ActionsProcessedAt, returnVal,
      actionLocation, piece);
    return returnVal;
  }


  public void StartGame() {
    GameEvents.AnnounceEvent(GameEvents.StartGame);
    // Start the first turn on player 0
    NextTurn(0);
    GameEvents.AnnounceEvent(GameEvents.GameStarted);
  }

  public bool PlayerInCheck(int playerNum) {
    if (playerNum < 0 || playerNum >= PlayerCheck.Length) {
      GD.PushError(
        $"Tried to check if player {playerNum + 1} is in check, when there are only {PlayerCheck.Length} players (0 - {PlayerCheck.Length - 1}.");
      return false;
    }

    return PlayerCheck[playerNum] == CheckType.InCheck ||
           PlayerCheck[playerNum] == CheckType.PossibleCheckmate;
  }

  public bool PlayerHasNoKing(int playerNum) {
    if (playerNum < 0 || playerNum >= PlayerCheck.Length) {
      GD.PushError(
        $"Tried to check if player {playerNum + 1} has no King, when there are only {PlayerCheck.Length} players (0 - {PlayerCheck.Length - 1}.");
      return false;
    }

    return PlayerCheck[playerNum] == CheckType.NoKing;
  }

  // Returns the id of the player in the next turn
  public int GetNextPlayerNumber() {
    int newPlayer = CurrentPlayerNum + 1;
    newPlayer %= NumberOfPlayers;
    return newPlayer;
  }

  public int EndTurn() {
    // First, end turn
    GameEvents.AnnounceEvent(GameEvents.EndTurn);
    int nextPlayer = GetNextPlayerNumber();
    CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.TurnEnded, CurrentPlayerNum,
      nextPlayer);
    return nextPlayer;
  }

  public void NextTurn(int newPlayerNum) {
    // If action grid was temporary, replace it with a new one
    if (_tempActionGrid) {
      Grid<ActionBase> oldActions = ActionGrid;
      ActionGrid = new Grid<ActionBase>();
      CallDeferred(Node.MethodName.AddChild, ActionGrid);

      // Re-add old actions if their piece doesn't need updating
      foreach (GridCell<ActionBase> cell in oldActions.Cells)
      foreach (GridItem<ActionBase> item in cell.Items) {
        var action = (ActionBase)item;
        if (TryGetPiece(action.Owner.Id, out Piece piece)) {
          if (!piece.NeedsActionUpdate) {
            ActionGrid.PlaceItemAt(action, action.ActionLocation.X, action.ActionLocation.Y, false);
          }
        }
      }
    }


    if (IsServer)
      // Tell all pieces that it's the next turn
    {
      foreach (Piece piece in AllPieces) {
        // Tell the piece it's the end of the turn. It's
        // possible that the piece will request actions to
        // be remade as a result.
        piece.EndTurn(this);
        // Remove all actions from the Grid if the piece needs updating
        if (piece.NeedsActionUpdate) {
          foreach (ActionBase action in piece.CurrentPossibleActions) {
            if (action.Cell != null && action.Owner == piece) {
              action.Cell.RemoveItem(action);
            }
          }

          piece.ClearActions();
        }
      }
    }

    // Move to the next player
    if (newPlayerNum <= -1) {
      newPlayerNum = CurrentPlayerNum + 1;
    }

    SetPlayerNum(newPlayerNum);

    // Tell all pieces that it's the next turn
    if (IsServer) {
      PiecesNewTurn();
    }

    GameEvents.AnnounceEvent(GameEvents.NewTurn);
    CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.TurnStarted, CurrentPlayerNum);
  }

  // Alternate function (for Godot to call)
  public void NextTurn() {
    NextTurn(-1);
  }


  public void AddCard(CardBase card, bool callCardAdd) {
    if (card.CardId == null) {
      GD.PushError(
        $"Could not add {card.GetType().Name} as it does not have a card id. Was it created through a Factory?");
      return;
    }

    // Add the card before anything.
    // Doing it in this order allows card data to be accessed by the main thread before it's freed.
    CallDeferred(Node.MethodName.AddChild, card);
    CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.CardAdded, card);

    // Before continuing, get all matching cards first and call the OnMatchingCard to see what
    // needs to be done with the card.
    foreach (CardBase existingCard in GetExistingCards(card.CardId)) {
      CardReturn result = existingCard.OnMatchingCard(card);
      // If the card is combined, the card should be freed as it is no longer going to be used.
      if (result.HasFlag(CardReturn.Combined)) {
        card.CallDeferred(Node.MethodName.QueueFree);
        return;
      }

      // If the card is disabled, set enabled to false. OnAddCard is still
      // called, so the individual cards need to only do things if enabled is true.
      bool disableBoth = result.HasFlag(CardReturn.DisableBoth);
      if (result.HasFlag(CardReturn.DisableNew) || disableBoth) {
        card.Enabled = false;
      }

      // Disable the existing card if it returns that it should be.
      if (result.HasFlag(CardReturn.DisableThis) || disableBoth) {
        existingCard.Enabled = false;
      }
    }

    // Immediate use cards don't need to add notices, or be included
    // in the array as they will be gone immediately after.
    if (!card.ImmediateUse) {
      card.MakeNotices(this);
      Cards.Add(card);
      card.CallDeferred(Node.MethodName.AddChild, card.CardNotices);
    }

    // If the card should only be processed on the server,
    // don't add listeners or call AddCard.
    bool processCard = !(card.ServerOnly && !IsServer);
    if (processCard) {
      if (!card.ImmediateUse) {
        card.MakeListeners(GameEvents);
      }

      if (callCardAdd) {
        card.OnAddCard(this);
      }
    }

    if (card.ImmediateUse) {
      card.CallDeferred(Node.MethodName.QueueFree);
    }
  }

  public bool RemoveCard(CardBase card) {
    if (!Cards.Remove(card)) {
      GD.PushWarning($"Tried to remove card {card.GetCardName()} when it is not added.");
      return false;
    }

    Cards.Remove(card);
    return true;
  }

  public void StartEventsWait() {
    Task waitEvent = GameEvents.Wait();
    waitEvent.Wait();
  }

  public void EndEventsWait() {
    GameEvents.EndWait();
  }

  public List<CardBase> GetExistingCards(string cardId) {
    var existingCards = new List<CardBase>();
    foreach (CardBase card in Cards) {
      if (card.CardId == cardId) {
        existingCards.Add(card);
      }
    }

    return existingCards;
  }


  public void AddActionRule(string ruleId, string pieceId = null) {
    if (!_gameController.ActionRuleRegistry.TryGetValue(ruleId, out ActionRuleBase rule)) {
      GD.PushError($"Tried to add an action rule {ruleId} when it has not been registered.");
      return;
    }

    // If target Piece is defined, add directly to the PieceInfo
    if (pieceId != null) {
      if (_gameController.PieceInfoRegistry.TryGetValue(pieceId, out PieceInfo info)) {
        info.AddActionRule(rule);
        return;
      }

      // If it didn't get the info, error
      GD.PushError($"Couldn't get the PieceInfo for piece id {pieceId}.");
      return;
    }

    // If no pieceId is defined, add it to all pieceIds
    foreach (PieceInfo info in _gameController.PieceInfoRegistry.GetValues()) {
      info.AddActionRule(rule);
    }
  }

  public void AddVerificationRule(string ruleId, string pieceId = null) {
    if (!_gameController.ValidationRuleRegistry.TryGetValue(ruleId, out ValidationRuleBase rule)) {
      GD.PushError($"Tried to add a verification rule {ruleId} when it has not been registered.");
      return;
    }

    // If target Piece is defined, add directly to the PieceInfo
    if (pieceId != null) {
      if (_gameController.PieceInfoRegistry.TryGetValue(pieceId, out PieceInfo info)) {
        info.AddValidationRule(rule);
        return;
      }

      // If it didn't get the info, error
      GD.PushError($"Couldn't get the PieceInfo for piece id {pieceId}.");
      return;
    }

    // If no pieceId is defined, add it to all pieceIds
    foreach (PieceInfo info in _gameController.PieceInfoRegistry.GetValues()) {
      info.AddValidationRule(rule);
    }
  }


  public void EnableActionUpdatesForPieceId(string pieceId) {
    foreach (Piece piece in AllPieces) {
      if (piece.GetPieceInfoId() == pieceId) {
        piece.EnableActionsUpdate();
      }
    }
  }


  public string[] GetAllPieceIds() {
    return _gameController.PieceInfoRegistry.GetKeys();
  }

  public List<string> GetPieceIdsOnBoard() {
    var existingPieceIds = new List<string>();

    // Get the id of the pieces currently in the game
    foreach (Piece piece in AllPieces) {
      string infoId = piece.GetPieceInfoId();
      if (existingPieceIds.Contains(infoId)) {
        continue;
      }

      existingPieceIds.Add(infoId);
    }

    return existingPieceIds;
  }

  public List<PieceInfo> GetPieceInfoOnBoard() {
    var existingPieceInfo = new List<PieceInfo>();

    // Get the info of the pieces currently in the game
    foreach (Piece piece in AllPieces) {
      if (piece.Info == null || existingPieceInfo.Contains(piece.Info)) {
        continue;
      }

      existingPieceInfo.Add(piece.Info);
    }

    return existingPieceInfo;
  }


  public GameState Clone() {
    // Initialise the new state
    var newState = new GameState(_gameController, NumberOfPlayers);
    newState.Init(IsServer);
    // Copy over the seed so any randomness follows the same
    newState.GameRandom.Seed = GameRandom.Seed;

    // Copy the Cards
    foreach (CardBase card in Cards) {
      newState.AddCard(card.Clone(), false);
    }

    // Copy over the pieces
    // TODO: Copy over PieceInfo, too.
    foreach (Piece piece in AllPieces) {
      Piece newPiece = piece.Clone();
      newState.AllPieces.Add(newPiece);
      newState.PieceGrid.PlaceItemAt(newPiece, piece.Cell.X, piece.Cell.Y);
      // Add all the items' actions
      foreach (ActionBase action in piece.CurrentPossibleActions) {
        newPiece.AddAction(action);
      }
    }

    // Copy over variables
    newState.CurrentPlayerNum = CurrentPlayerNum;
    newState.GridUpperCorner = GridUpperCorner;
    newState.GridLowerCorner = GridLowerCorner;
    newState._lastId = _lastId;
    newState.KingId = KingId;

    return newState;
  }


  // Internal Methods

  internal void SendCardNotice(CardBase card, string notice) {
    CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.CardNotice, card, notice);
  }

  internal void ReceiveCardNotice(CardBase card, string notice) {
    // Tell the current active card (if there is one).
    if (Cards.Contains(card)) {
      card.ReceiveNotice(notice);
    }
  }


  // Private Methods


  private bool DoActionsAt(Vector2I actionLocation, Piece piece) {
    if (piece == null) {
      return false;
    }

    bool didAct = false;
    // Loop through all actions, and find the ones at x, y.
    if (ActionGrid.TryGetCellAt(actionLocation, out GridCell<ActionBase> actionCell)) {
      var actions = new List<ActionBase>();
      foreach (GridItem<ActionBase> actionGridItem in actionCell.Items) {
        var action = (ActionBase)actionGridItem;
        if (action.Owner != null && action.Owner.Id == piece.Id) {
          actions.Add(action);
        }
      }

      for (int i = 0; i < actions.Count; i++) {
        // If the piece is taken (or invalid due to running on a thread), break from the loop.
        if (!IsInstanceValid(piece) || piece.Taken) {
          GD.PushWarning(
            "A Piece had more Actions to take, even after having taken itself out from the game: " +
            $"[{string.Join(", ", actions.GetRange(i, actions.Count - i))}]");
          break;
        }

        ActionBase action = actions[i];
        didAct |= TakeAction(action, piece);
      }
    }
    else {
      GD.Print(
        $"Tried to make Piece {piece.Id} take Actions at {actionLocation}, but the GridCell doesn't exist.");
    }

    return didAct;
  }

  private void PiecesNewTurn() {
    // Announce that a new turn is about to start
    GameEvents.AnnounceEvent(GameEvents.PreNewTurn);
    foreach (Piece piece in AllPieces) {
      bool addToGrid = piece.NeedsActionUpdate;
      // Update all the actions
      piece.UpdateActions(this);
      // Add all the actions to the Grid, if they need to be.
      if (addToGrid) {
        foreach (ActionBase action in piece.CurrentPossibleActions) {
          ActionGrid.PlaceItemAt(action, action.ActionLocation.X, action.ActionLocation.Y);
        }
      }

      piece.NewTurn(this);
    }

    // Reset player check
    for (int i = 0; i < PlayerCheck.Length; i++) {
      PlayerCheck[i] = CheckType.None;
    }

    // Loop again, to disable certain check moves
    int[] kingCount = new int[NumberOfPlayers];

    var kings = new List<Piece>();

    // Find all the kings
    foreach (Piece piece in AllPieces) {
      if (piece.Info == null || piece.Info.PieceId != KingId) {
        continue;
      }

      kings.Add(piece);
      kingCount[piece.TeamId] += 1;
    }

    // Loop through kings to check for Check
    foreach (Piece king in kings) {
      // If there are too many kings for its team, ignore
      if (kingCount[king.TeamId] > 1) {
        continue;
      }

      // If it's the king, disable any attacks on it and stop it from moving into
      // a space with check
      if (ActionGrid.TryGetCellAt(king.Cell.X, king.Cell.Y, out GridCell<ActionBase> cell)) {
        foreach (GridItem<ActionBase> item in cell.Items) {
          if (item is AttackAction attackAction) {
            // If invalid, or not acting, ignore the attack
            if (!attackAction.Valid || !attackAction.Acting) {
              continue;
            }

            // If attack is valid, and is able to check, then mark it down for the player
            if (PlayerCheck[king.TeamId] == CheckType.None) {
              if (attackAction.Owner.TeamId != king.TeamId) {
                if (!attackAction.VerifyTags.Contains("no_check")) {
                  PlayerCheck[king.TeamId] = CheckType.InCheck;
                }
              }
            }

            // Once done, make the attack action, and a move action if it has it, invalid
            // as the king can't be attacked.
            attackAction.MakeInvalid();
            if (attackAction.MoveAction != null) {
              attackAction.MoveAction.MakeInvalid();
            }
          }
        }
      }
    }

    // Stop temp states here as they don't need to do the following checks
    if (_tempState) {
      return;
    }

    // Go through the Kings again, and disable moves that would move into
    // check.
    foreach (Piece king in kings) {
      // If there are too many kings for its team, ignore
      if (kingCount[king.TeamId] > 1) {
        continue;
      }

      System.Collections.Generic.Dictionary<Vector2I, List<ActionBase>> locationToAction = new();
      foreach (ActionBase action in king.CurrentPossibleActions) {
        // If the action isn't valid, ignore
        if (!action.Valid) {
          continue;
        }

        if (!locationToAction.TryGetValue(action.ActionLocation, out List<ActionBase> actions)) {
          actions = new List<ActionBase>();
          locationToAction.Add(action.ActionLocation, actions);
        }

        actions.Add(action);
      }

      bool canMove = false;
      foreach (KeyValuePair<Vector2I, List<ActionBase>> location in locationToAction) {
        if (DoesActionCheck(location.Key, king))
          // Disable all actions at this location if it does check the King, but
          // only the actions at this location.
        {
          foreach (ActionBase action in location.Value) {
            action.MakeInvalid();
          }
        }
        else {
          canMove = true;
        }
      }

      if (!canMove) {
        if (PlayerCheck[king.TeamId] == CheckType.InCheck) {
          PlayerCheck[king.TeamId] = CheckType.PossibleCheckmate;
        }
        else {
          PlayerCheck[king.TeamId] = CheckType.PossibleStalemate;
        }
      }
    }

    // If it's non-check, ignore as server already did the check
    if (!IsServer) {
      return;
    }

    // Check if either player is missing a King
    // This is prioritised over being in Checkmate, as having no King means Checkmate
    // isn't possible
    for (int teamNum = 0; teamNum < NumberOfPlayers; teamNum++) {
      if (kingCount[teamNum] == 0) {
        // Signal that the player has lost.
        PlayerCheck[teamNum] =
          CheckType.NoKing; // Put player as having No King, so that checkmate isn't checked for
        CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.SendNotice, -1,
          $"With no {GetPieceInfo(KingId)?.DisplayName}, Player {teamNum + 1}'s army is lost.");
        CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.PlayerLost, teamNum);
      }
    }

    // Check if each player is in check
    bool checkmate = false;
    for (int teamNum = 0; teamNum < NumberOfPlayers; teamNum++) {
      // If the team is not the one playing, it means they won't be able to move anyway
      if (teamNum != CurrentPlayerNum) {
        continue;
      }

      // No need to check for King in Checkmate if there is no King
      if (kingCount[teamNum] == 0) {
        continue;
      }

      // If they are playing, then check if they're in check or checkmate
      if (PlayerCheck[teamNum] == CheckType.None)
        // Check can be ignored, as it means the King, at least, can move out of Check
      {
        break;
      }

      // If King can move, they have an action they can take
      // TODO: Certain cards may change this fact. King movement check will need to take Cards into account by
      // checking if MoveAction checks, rather than if the King is in Checkmate.
      if (PlayerCheck[teamNum] != CheckType.PossibleCheckmate) {
        break;
      }

      CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.SendNotice, -1,
        "Checking for Checkmate.");

      // If the King is possibly in Checkmate, each possible action needs to be checked.
      bool foundNoCheck = false;
      foreach (Piece piece in AllPieces) {
        // Only check for the current team being checked, and if the piece has info
        if (piece.Info == null || piece.TeamId != teamNum) {
          continue;
        }

        // For each piece, we need to check if the piece can make any action that results
        // in no check. If there is a SINGLE action, then we can stop checking. Otherwise, we continue.
        // In the instance that the player is in checkmate, they are out of the game.
        var checkedLocations = new HashSet<Vector2I>();
        foreach (ActionBase action in piece.CurrentPossibleActions) {
          // If action is invalid, then ignore it
          if (!action.Valid || !action.Acting) {
            continue;
          }

          // If action location is already done, ignore
          if (!checkedLocations.Add(action.ActionLocation)) {
            continue;
          }

          if (!DoesActionCheck(action.ActionLocation, piece)) {
            foundNoCheck = true;
            break;
          }
        }

        if (foundNoCheck) {
          break;
        }
      }

      // If no check was found, then the player has lost.
      checkmate = !foundNoCheck;
      if (foundNoCheck) {
        CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.SendNotice, -1,
          "Not Checkmate.");
      }
    }

    if (checkmate) {
      CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.SendNotice, -1, "Checkmate!");
      CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.PlayerLost, CurrentPlayerNum);
    }

    // Check if the game is in Stalemate
    // Check if the current playing team is possibly in stalemate.
    if (PlayerCheck[CurrentPlayerNum] == CheckType.PossibleStalemate) {
      bool isStalemate = true;
      // Check all piece actions
      foreach (Piece piece in AllPieces) {
        // Only check the current team
        if (piece.TeamId != CurrentPlayerNum) {
          continue;
        }

        // If there are any valid actions, then the game is not in stalemate.
        List<Vector2I> checkLocations = new();
        foreach (ActionBase action in piece.CurrentPossibleActions) {
          // If the location is already checked, or the action is invalid, ignore
          if (checkLocations.Contains(action.ActionLocation) || !action.Valid) {
            continue;
          }

          checkLocations.Add(action.ActionLocation);
          // Only continue if the move does not result in Check. This is neeeded,
          // otherwise it will accept moves that aren't possible due to them putting
          // the player in to Check.
          if (!DoesActionCheck(action.ActionLocation, piece)) {
            isStalemate = false;
            break;
          }
        }

        // If it's not stalemate, break out of the piece loop.
        if (!isStalemate) {
          break;
        }
      }

      // If isStalemate is still true, the game is over.
      if (isStalemate) {
        CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.SendNotice, -1, "Stalemate.");
        CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.GameStalemate, CurrentPlayerNum);
      }
    }
  }

  private void RestoreValidation() {
    foreach (Piece piece in AllPieces)
    foreach (ActionBase action in piece.CurrentPossibleActions) {
      action.SetValidToStored();
    }
  }

  private void StoreCurrentValidation() {
    foreach (Piece piece in AllPieces)
    foreach (ActionBase action in piece.CurrentPossibleActions) {
      action.StoredValidation = action.Valid;
    }
  }
}