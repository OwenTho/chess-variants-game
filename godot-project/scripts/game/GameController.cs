using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Mutex = Godot.Mutex;

[GlobalClass]
public partial class GameController : Node {
  // Public Fields

  public bool SingleThread = false;


  // The current state of the game.
  public GameState CurrentGameState { get; private set; }

  // A copy of the current GameState; This is used when currentGameState is currently working
  public GameState GameStateCopy { get; private set; }
  public Semaphore ThreadSemaphore { get; private set; } = new();
  public Mutex TaskMutex { get; private set; } = new();
  public Mutex ThreadMutex { get; private set; } = new();
  public Mutex GameMutex { get; private set; } = new();

  // Private Fields
  private readonly List<Action> _gameTasks = new();
  private GodotThread _gameThread;
  private bool _stillTasking = true;
  private bool _doneFree;

  public Vector2I GridUpperCorner => CurrentGameState.GridUpperCorner;
  public Vector2I GridLowerCorner => CurrentGameState.GridLowerCorner;


  // Grid is also unchanging, only changing its contents. Mutex only has to be
  // used as needed when altering / using contents.
  public Grid<Piece> PieceGrid => CurrentGameState.PieceGrid;

  public Array<string> GetPieceKeys() {
    string[] keys = PieceInfoRegistry.GetKeys();
    var pieceKeys = new Array<string>();
    foreach (string key in keys) {
      pieceKeys.Add(key);
    }

    return pieceKeys;
  }

  public PieceInfo GetPieceInfo(string key) {
    return PieceInfoRegistry.GetValue(key);
  }

  public bool TryGetPieceInfo(string key, out PieceInfo info) {
    info = GetPieceInfo(key);
    return info != null;
  }

  public string GetPieceName(string key) {
    if (TryGetPieceInfo(key, out PieceInfo info)) {
      return info.DisplayName;
    }

    return "Invalid Piece ID";
  }

  public RuleBase GetRule(string key) {
    return ActionRuleRegistry.GetValue(key);
  }

  public string GetActionId(ActionBase action) {
    foreach (ActionFactory actionFactory in ActionFactoriesRegistry.GetValues()) {
      if (actionFactory.ActionTypeMatches(action)) {
        return actionFactory.ActionId;
      }
    }

    return null;
  }


  public Array<Piece> GetAllPieces() {
    if (CurrentGameState == null) {
      return null;
    }

    return CurrentGameState.AllPieces;
  }


  public CardBase PullCardFromDeck(CardDeck deck) {
    GameMutex.Lock();
    CardBase newCard = deck.PullCard(CurrentGameState);
    GameMutex.Unlock();
    return newCard;
  }

  public bool ReturnCardToDeck(CardBase card, CardDeck deck) {
    return deck.PutCard(card);
  }


  public CardBase PullMajorCard() {
    if (MajorCardDeck == null) {
      return null;
    }

    return MajorCardDeck.PullCard(CurrentGameState);
  }

  public void ReturnMajorCard(CardBase card) {
    if (MajorCardDeck == null || card == null) {
      return;
    }

    MajorCardDeck.PutCard(card);
  }


  public CardBase MakeCard(string cardId) {
    if (!CardFactoryRegistry.TryGetValue(cardId, out CardFactory cardFactory)) {
      GD.PushError(
        $"Tried to Make a card of id {cardId}, but no CardFactory is registered for it.");
      return null;
    }

    // Lock game mutex as it's using game data
    GameMutex.Lock();
    CardBase newCard = cardFactory.CreateNewCard(CurrentGameState);
    GameMutex.Unlock();

    return newCard;
  }

  public CardBase MakeCardUsingFactory(CardFactory cardFactory) {
    GameMutex.Lock();
    CardBase newCard = null;
    if (cardFactory.CanMakeNewCard(CurrentGameState)) {
      newCard = cardFactory.CreateNewCard(CurrentGameState);
    }

    GameMutex.Unlock();

    return newCard;
  }

  public CardBase MakeCardFromDict(Godot.Collections.Dictionary<string, string> cardData) {
    if (cardData == null) {
      return null;
    }

    // If the cardData has no id, ignore
    if (!cardData.TryGetValue("card_id", out string cardId)) {
      GD.PushError("Tried to make a card from a dictionary, but no card_id was provided.");
      return null;
    }

    // First, get the factory
    if (!CardFactoryRegistry.TryGetValue(cardId, out CardFactory cardFactory)) {
      GD.PushError(
        $"Tried to Make a card of id {cardId}, but no CardFactory is registered for it.");
      return null;
    }

    // Lock game mutex as it's using game data
    GameMutex.Lock();
    CardBase newCard = cardFactory.CreateNewCardFromDict(CurrentGameState, cardData);
    GameMutex.Unlock();

    return newCard;
  }

  public string GetCardName(CardBase card) {
    return card.GetCardName(CurrentGameState);
  }

  public string GetCardImageLoc(CardBase card) {
    return card.GetCardImageLoc(CurrentGameState);
  }

  public string GetCardDescription(CardBase card) {
    return card.GetCardDescription(CurrentGameState);
  }

  public Godot.Collections.Dictionary<string, string> ConvertCardToDict(CardBase card) {
    if (card == null) {
      return null;
    }

    // Lock game mutex as it's using game data
    GameMutex.Lock();
    Godot.Collections.Dictionary<string, string> cardData = card.ConvertToDict(CurrentGameState);
    GameMutex.Unlock();

    return cardData;
  }

  public void AddCardTask(CardBase card) {
    GameMutex.Lock();
    CurrentGameState.AddCard(card, true);
    GameMutex.Unlock();
  }

  public void AddCard(CardBase card) {
    // For now, simply add the card to the game
    DoTask(() => AddCardTask(card));
  }

  public void SendCardNotice(CardBase card, string notice) {
    CurrentGameState.ReceiveCardNotice(card, notice);
  }


  private void SetGameSeedTask(ulong seed) {
    GameMutex.Lock();
    CurrentGameState.GameRandom.Seed = seed;
    GameMutex.Unlock();
  }

  public void SetGameSeed(ulong seed) {
    DoTask(() => SetGameSeedTask(seed));
  }

  public ulong GetGameSeed() {
    GameMutex.Lock();
    ulong returnVal = CurrentGameState.GameRandom.Seed;
    GameMutex.Unlock();
    return returnVal;
  }

  private void SetGameSeedStateTask(ulong state) {
    GameMutex.Lock();
    CurrentGameState.GameRandom.State = state;
    GameMutex.Unlock();
  }

  public void SetGameSeedState(ulong state) {
    DoTask(() => SetGameSeedStateTask(state));
  }

  public ulong GetGameSeedState() {
    GameMutex.Lock();
    ulong returnVal = CurrentGameState.GameRandom.State;
    GameMutex.Unlock();
    return returnVal;
  }

  private void StartGameTask(ulong seed) {
    GameMutex.Lock();

    CurrentGameState.GameRandom.Seed = seed;

    CurrentGameState.StartGame();
    GameMutex.Unlock();
  }

  public void StartGame(ulong seed) {
    DoTask(() => StartGameTask(seed));
  }

  public void StartGame() {
    // If called without the seed, use the current seed
    StartGame(CurrentGameState.GameRandom.Seed);
  }


  public int UnsafeGetCurrentPlayer() {
    return CurrentGameState.CurrentPlayerNum;
  }

  public int GetCurrentPlayer() {
    GameMutex.Lock();
    int curPlayer = UnsafeGetCurrentPlayer();
    GameMutex.Unlock();
    return curPlayer;
  }

  public Array<int> UnsafeGetPlayersInCheck() {
    return CurrentGameState.GetPlayersInCheck();
  }

  public Array<int> GetPlayersInCheck() {
    GameMutex.Lock();
    Array<int> playersInCheck = UnsafeGetPlayersInCheck();
    GameMutex.Unlock();
    return playersInCheck;
  }


  private Piece PlacePiece(string pieceId, int linkId, int teamId, int x, int y, int id = -1) {
    GameMutex.Lock();
    Piece returnPiece = CurrentGameState.PlacePiece(pieceId, linkId, teamId, x, y, id);
    GameMutex.Unlock();
    return returnPiece;
  }

  public void MovePiece(Piece piece, int x, int y) {
    CurrentGameState.MovePiece(piece, x, y);
  }

  public void MovePieceId(int piece, int x, int y) {
    CurrentGameState.MovePiece(piece, x, y);
  }

  public void TakePiece(Piece piece, Piece attacker) {
    CurrentGameState.TakePiece(piece, attacker);
  }

  public void TakePieceId(int piece, int attackerId) {
    CurrentGameState.TakePiece(piece, attackerId);
  }

  public void SwapPieceTo(int pieceId, string infoId) {
    SwapPieceTo(GetPiece(pieceId), infoId);
  }

  public void SwapPieceTo(Piece piece, string infoId) {
    if (piece == null) {
      return;
    }

    if (TryGetPieceInfo(infoId, out PieceInfo info)) {
      piece.Info = info;
    }
  }


  public Piece UnsafeGetPiece(int pieceId) {
    return CurrentGameState.GetPiece(pieceId);
  }

  public Piece GetPiece(int pieceId) {
    GameMutex.Lock();
    Piece returnValue = UnsafeGetPiece(pieceId);
    GameMutex.Unlock();
    return returnValue;
  }

  public Array<Piece> UnsafeGetKingPieces() {
    return CurrentGameState.GetKingPieces();
  }

  public Array<Piece> GetKingPieces() {
    GameMutex.Lock();
    Array<Piece> returnValue = UnsafeGetKingPieces();
    GameMutex.Unlock();
    return returnValue;
  }

  public Piece UnsafeGetFirstPieceAt(int x, int y) {
    return CurrentGameState.GetFirstPieceAt(x, y);
  }

  public Piece GetFirstPieceAt(int x, int y) {
    GameMutex.Lock();
    Piece returnPiece = UnsafeGetFirstPieceAt(x, y);
    GameMutex.Unlock();
    return returnPiece;
  }

  public Array<Piece> UnsafeGetPiecesByLinkId(int linkId) {
    return CurrentGameState.GetPiecesByLinkId(linkId);
  }

  public Array<Piece> GetPiecesByLinkId(int linkId) {
    GameMutex.Lock();
    Array<Piece> returnPieces = UnsafeGetPiecesByLinkId(linkId);
    GameMutex.Unlock();
    return returnPieces;
  }


  public bool IsActionValid(ActionBase action, Piece piece) {
    GameMutex.Lock();
    bool result = CurrentGameState.IsActionValid(action, piece);
    GameMutex.Unlock();
    return result;
  }


  private void TakeActionTask(ActionBase action, Piece piece) {
    GameMutex.Lock();
    CurrentGameState.TakeAction(action, piece);
    GameMutex.Unlock();
  }

  public void TakeAction(ActionBase action, Piece piece) {
    if (action == null) {
      return;
    }

    DoTask(() => TakeActionTask(action, piece));
  }

  private void TakeActionAtTask(Vector2I actionLocation, Piece piece) {
    GameMutex.Lock();
    CurrentGameState.TakeActionAt(actionLocation, piece);
    GameMutex.Unlock();
  }

  public void TakeActionAt(Vector2I actionLocation, Piece piece) {
    DoTask(() => TakeActionAtTask(actionLocation, piece));
  }


  public Godot.Collections.Dictionary<string, string> ActionToDict(ActionBase action) {
    return action.ConvertToDict(CurrentGameState);
  }

  public ActionBase ActionFromDict(Godot.Collections.Dictionary<string, string> actionDict) {
    if (actionDict.TryGetValue("action_id", out string actionId)) {
      if (ActionFactoriesRegistry.TryGetValue(actionId, out ActionFactory actionFactory)) {
        return actionFactory.CreateNewActionFromDict(actionDict);
      }

      GD.PushError($"Could not find an ActionFactory for {actionId}.");
    }
    else {
      GD.PushError("Action dictionary missing action_id.");
    }

    return null;
  }


  public void NextTurn(int playerNumber) {
    // Call on a thread, so that it's faster
    DoTask(() => CurrentGameState.NextTurn(playerNumber));
  }

  // Function for Godot
  public void NextTurn() {
    NextTurn(-1);
  }

  public void EndTurn() {
    // Call on the game thread
    DoTask(() => CurrentGameState.EndTurn());
  }


  // Function calls to the currentGameState for easier access
  private void NextTask() {
    // Thread mutex locks before checking "stillTasking" so that it can be read without issue.
    ThreadMutex.Lock();
    while (_stillTasking) {
      // Close thread mutex before, just in case a task is added here
      TaskMutex.Lock();

      // If there are no tasks, stop the thread
      if (_gameTasks.Count == 0) {
        // Close thread
        TaskMutex.Unlock();
        ThreadMutex.Unlock();
        ThreadSemaphore.Wait();
        ThreadMutex.Lock();
        continue;
      }

      ThreadMutex.Unlock();

      // If there are tasks, run the latest one
      Action taskToDo = _gameTasks[0];
      _gameTasks.RemoveAt(0);
      TaskMutex.Unlock();

      // GD.Print($"Doing next task: {taskToDo.GetMethodInfo().Name}");
      taskToDo.Invoke();
      // GD.Print($"Task Complete ({taskToDo.GetMethodInfo().Name}), New total: {gameTasks.Count}");
      ThreadMutex.Lock();
    }
  }

  private void DoTask(Action task) {
    // If using a single thread, just run the task
    if (SingleThread) {
      // If there are Tasks in the queue, wait until they are done
      while (_gameTasks.Count > 0) {
      }

      task();
      return;
    }

    // If there's currently a task running, add to tasks and return
    TaskMutex.Lock();
    _gameTasks.Add(task);
    // GD.Print($"New Task ({task.GetMethodInfo().Name}), Total tasks: {gameTasks.Count}");
    ThreadMutex.Lock();
    if (_gameThread != null) {
      ThreadMutex.Unlock();
      TaskMutex.Unlock();
      ThreadSemaphore.Post();
      return;
    }

    TaskMutex.Unlock();

    // If there is no task running, start a new thread
    _gameThread = new GodotThread();
    ThreadMutex.Unlock();

    _gameThread.Start(Callable.From(NextTask));
  }

  public override void _Notification(int what) {
    if (what == NotificationPredelete) {
      // Clear task list and wait for thread to finish
      _gameTasks.Clear();
      ThreadMutex.Lock();
      _stillTasking = false;
      ThreadMutex.Unlock();
      ThreadSemaphore.Post();
      if (CurrentGameState != null) {
        // End the GameEvents wait, if it's currently happening
        if (CurrentGameState.GameEvents != null) {
          CurrentGameState.GameEvents.CanWait = false;
        }

        CurrentGameState.EndEventsWait();
      }

      // Wait for the thread to stop
      if (_gameThread != null) {
        _gameThread.WaitToFinish();
      }

      if (CurrentGameState != null) {
        if (IsInstanceValid(CurrentGameState)) {
          CurrentGameState.Free();
        }
      }

      if (GameStateCopy != null) {
        if (IsInstanceValid(GameStateCopy)) {
          GameStateCopy.Free();
        }
      }
    }
  }
}