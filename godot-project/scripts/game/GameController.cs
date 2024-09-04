using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Mutex = Godot.Mutex;

public partial class GameController : Node
{
    // The current state of the game.
    public GameState currentGameState { get; private set; }
    // A copy of the current GameState; This is used when currentGameState is currently working
    public GameState gameStateCopy { get; private set; }

    private bool stillTasking = true;
    public Semaphore threadSemaphore { get; private set; } = new Semaphore();
    public Mutex taskMutex { get; private set; } = new Mutex();
    public Mutex threadMutex { get; private set; } = new Mutex();
    public Mutex gameMutex { get; private set; } = new Mutex();
    private List<Action> gameTasks = new List<Action>();

    private GodotThread gameThread;
    public bool singleThread = false;
    
    
    public Vector2I gridUpperCorner => currentGameState.gridUpperCorner;
    public Vector2I gridLowerCorner => currentGameState.gridLowerCorner;
    
    
    // Grid is also unchanging, only changing its contents. Mutex only has to be
    // used as needed when altering / using contents.
    public Grid<Piece> pieceGrid => currentGameState.pieceGrid;

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

    public bool TryGetPieceInfo(string key, out PieceInfo info)
    {
        info = GetPieceInfo(key);
        return info != null;
    }

    public RuleBase GetRule(string key)
    {
        return actionRuleRegistry.GetValue(key);
    }

    public string GetActionId(ActionBase action)
    {
        foreach (var actionFactory in actionFactoriesRegistry.GetValues())
        {
            if (actionFactory.ActionTypeMatches(action))
            {
                return actionFactory.actionId;
            }
        }

        return null;
    }



    public Array<Piece> GetAllPieces()
    {
        if (currentGameState == null)
        {
            return null;
        }

        return currentGameState.allPieces;
    }



    public CardBase PullCardFromDeck(CardDeck deck)
    {
        gameMutex.Lock();
        CardBase newCard = deck.PullCard(currentGameState);
        gameMutex.Unlock();
        return newCard;
    }
    
    public bool ReturnCardToDeck(CardBase card, CardDeck deck)
    {
        return deck.PutCard(card);
    }


    public CardBase PullMajorCard()
    {
        if (MajorCardDeck == null)
        {
            return null;
        }

        return MajorCardDeck.PullCard(currentGameState);
    }

    public void ReturnMajorCard(CardBase card)
    {
        if (MajorCardDeck == null || card == null)
        {
            return;
        }

        MajorCardDeck.PutCard(card);
    }

    
    public CardBase MakeCard(string cardId)
    {
        if (!cardFactoryRegistry.TryGetValue(cardId, out CardFactory cardFactory))
        {
            GD.PushError($"Tried to Make a card of id {cardId}, but no CardFactory is registered for it.");
            return null;
        }
        
        // Lock game mutex as it's using game data
        gameMutex.Lock();
        CardBase newCard = cardFactory.CreateNewCard(currentGameState);
        gameMutex.Unlock();

        return newCard;
    }

    public CardBase MakeCardFromDict(Godot.Collections.Dictionary<string, string> cardData)
    {
        if (cardData == null)
        {
            return null;
        }
        // If the cardData has no id, ignore
        if (!cardData.TryGetValue("card_id", out string cardId))
        {
            GD.PushError("Tried to make a card from a dictionary, but no card_id was provided.");
            return null;
        }
        // First, get the factory
        if (!cardFactoryRegistry.TryGetValue(cardId, out CardFactory cardFactory))
        {
            GD.PushError($"Tried to Make a card of id {cardId}, but no CardFactory is registered for it.");
            return null;
        }
        
        // Lock game mutex as it's using game data
        gameMutex.Lock();
        CardBase newCard = cardFactory.CreateNewCardFromDict(currentGameState, cardData);
        gameMutex.Unlock();

        return newCard;
    }

    public Godot.Collections.Dictionary<string, string> ConvertCardToDict(CardBase card)
    {
        if (card == null)
        {
            return null;
        }

        // Lock game mutex as it's using game data
        gameMutex.Lock();
        Godot.Collections.Dictionary<string, string> cardData = card.ConvertToDict(currentGameState);
        gameMutex.Unlock();
        
        return cardData;
    }

    public void AddCard(CardBase card)
    {
        // For now, simply add the card to the game
        currentGameState.AddCard(card, true);
    }

    public void SendCardNotice(CardBase card, string notice)
    {
        currentGameState.ReceiveCardNotice(card, notice);
    }




    
    private void SetGameSeedTask(ulong seed)
    {
        gameMutex.Lock();
        currentGameState.gameRandom.Seed = seed;
        gameMutex.Unlock();
    }
    
    public void SetGameSeed(ulong seed)
    {
        DoTask(() => SetGameSeedTask(seed));
    }

    public ulong GetGameSeed()
    {
        gameMutex.Lock();
        ulong returnVal = currentGameState.gameRandom.Seed;
        gameMutex.Unlock();
        return returnVal;
    }

    private void SetGameSeedStateTask(ulong state)
    {
        gameMutex.Lock();
        currentGameState.gameRandom.State = state;
        gameMutex.Unlock();
    }

    public void SetGameSeedState(ulong state)
    {
        DoTask(() => SetGameSeedStateTask(state));
    }

    public ulong GetGameSeedState()
    {
        gameMutex.Lock();
        ulong returnVal = currentGameState.gameRandom.State;
        gameMutex.Unlock();
        return returnVal;
    }

    private void StartGameTask(ulong seed)
    {
        gameMutex.Lock();
        
        currentGameState.gameRandom.Seed = seed;
        
        currentGameState.StartGame();
        gameMutex.Unlock();
    }

    public void StartGame(ulong seed)
    {
        DoTask(() => StartGameTask(seed));
    }

    public void StartGame()
    {
        // If called without the seed, use the current seed
        StartGame(currentGameState.gameRandom.Seed);
    }

    public int GetCurrentPlayer()
    {
        gameMutex.Lock();
        int curPlayer = currentGameState.currentPlayerNum;
        gameMutex.Unlock();
        return curPlayer;
    }

    public int UnsafeGetCurrentPlayer()
    {
        return currentGameState.currentPlayerNum;
    }

    
    private Piece PlacePiece(string pieceId, int linkId, int teamId, int x, int y, int id = -1)
    {
        gameMutex.Lock();
        Piece returnPiece = currentGameState.PlacePiece(pieceId, linkId, teamId, x, y, id);
        gameMutex.Unlock();
        return returnPiece;
    }

    public void MovePiece(Piece piece, int x, int y)
    {
        currentGameState.MovePiece(piece, x, y);
    }

    public void MovePieceId(int piece, int x, int y)
    {
        currentGameState.MovePiece(piece, x, y);
    }

    public void TakePiece(Piece piece, Piece attacker)
    {
        currentGameState.TakePiece(piece, attacker);
    }

    public void TakePieceId(int piece, int attackerId)
    {
        currentGameState.TakePiece(piece, attackerId);
    }

    public void SwapPieceTo(int piece_id, string info_id)
    {
        SwapPieceTo(GetPiece(piece_id), info_id);
    }

    public void SwapPieceTo(Piece piece, string info_id)
    {
        if (piece == null)
        {
            return;
        }

        if (TryGetPieceInfo(info_id, out PieceInfo info))
        {
            piece.info = info;
        }
    }
    
    
    public Piece GetPiece(int pieceId)
    {
        gameMutex.Lock();
        Piece returnValue = currentGameState.GetPiece(pieceId);
        gameMutex.Unlock();
        return returnValue;
    }

    public Piece UnsafeGetPiece(int pieceId)
    {
        return currentGameState.GetPiece(pieceId);
    }

    public Piece GetFirstPieceAt(int x, int y)
    {
        gameMutex.Lock();
        Piece returnPiece = currentGameState.GetFirstPieceAt(x, y);
        gameMutex.Unlock();
        return returnPiece;
    }




    public bool IsActionValid(ActionBase action, Piece piece)
    {
        gameMutex.Lock();
        bool result = currentGameState.IsActionValid(action, piece);
        gameMutex.Unlock();
        return result;
    }


    private void TakeActionTask(ActionBase action, Piece piece)
    {
        gameMutex.Lock();
        currentGameState.TakeAction(action, piece);
        gameMutex.Unlock();
    }
    
    public void TakeAction(ActionBase action, Piece piece)
    {
        if (action == null)
        {
            return;
        }
        DoTask(() => TakeActionTask(action, piece));
    }

    private void TakeActionAtTask(Vector2I actionLocation, Piece piece)
    {
        gameMutex.Lock();
        currentGameState.TakeActionAt(actionLocation, piece);
        gameMutex.Unlock();
    }

    public void TakeActionAt(Vector2I actionLocation, Piece piece)
    {
        DoTask(() => TakeActionAtTask(actionLocation, piece));
    }



    public Godot.Collections.Dictionary<string, string> ActionToDict(ActionBase action)
    {
        return action.ConvertToDict(currentGameState);
    }

    public ActionBase ActionFromDict(Godot.Collections.Dictionary<string, string> actionDict)
    {
        if (actionDict.TryGetValue("action_id", out string actionId))
        {
            if (actionFactoriesRegistry.TryGetValue(actionId, out ActionFactory actionFactory))
            {
                return actionFactory.CreateNewActionFromDict(actionDict);
            }
            GD.PushError($"Could not find an ActionFactory for {actionId}.");
        }
        else
        {
            GD.PushError("Action dictionary missing action_id.");
        }
        return null;
    }

    
    
    public void NextTurn(int playerNumber)
    {
        // Call on a thread, so that it's faster
        DoTask(() => currentGameState.NextTurn(playerNumber));
    }

    // Function for Godot
    public void NextTurn()
    {
        NextTurn(-1);
    }
    
    
    
    
    // Function calls to the currentGameState for easier access
    private void NextTask()
    {
        // Thread mutex locks before checking "stillTasking" so that it can be read without issue.
        threadMutex.Lock();
        while (stillTasking)
        {
            // Close thread mutex before, just in case a task is added here
            taskMutex.Lock();
            
            // If there are no tasks, stop the thread
            if (gameTasks.Count == 0)
            {
                // Close thread
                taskMutex.Unlock();
                threadMutex.Unlock();
                threadSemaphore.Wait();
                threadMutex.Lock();
                continue;
            }
            threadMutex.Unlock();

            // If there are tasks, run the latest one
            Action taskToDo = gameTasks[0];
            gameTasks.RemoveAt(0);
            taskMutex.Unlock();

            // GD.Print($"Doing next task: {taskToDo.GetMethodInfo().Name}");
            taskToDo.Invoke();
            // GD.Print($"Task Complete ({taskToDo.GetMethodInfo().Name}), New total: {gameTasks.Count}");
            threadMutex.Lock();
        }
    }
    
    private void DoTask(Action task)
    {
        // If using a single thread, just run the task
        if (singleThread)
        {
            // If there are Tasks in the queue, wait until they are done
            while (gameTasks.Count > 0)
            {
                
            }
            task();
            return;
        }
        // If there's currently a task running, add to tasks and return
        taskMutex.Lock();
        gameTasks.Add(task);
        // GD.Print($"New Task ({task.GetMethodInfo().Name}), Total tasks: {gameTasks.Count}");
        threadMutex.Lock();
        if (gameThread != null)
        {
            threadMutex.Unlock();
            taskMutex.Unlock();
            threadSemaphore.Post();
            return;
        }
        taskMutex.Unlock();
        
        // If there is no task running, start a new thread
        gameThread = new GodotThread();
        threadMutex.Unlock();
        
        gameThread.Start(Callable.From(NextTask));
    }


    private bool doneFree = false;
    public override void _Notification(int what)
    {
        if (what == NotificationPredelete)
        {
            // Clear task list and wait for thread to finish
            gameTasks.Clear();
            threadMutex.Lock();
            stillTasking = false;
            threadMutex.Unlock();
            threadSemaphore.Post();
            // End the GameEvents wait, if it's currently happening
            currentGameState.EndEventsWait();
            // Wait for the thread to stop
            if (gameThread != null)
            {
                gameThread.WaitToFinish();
            }

            if (currentGameState != null)
            {
                if (IsInstanceValid(currentGameState))
                {
                    currentGameState.Free();
                }
            }

            if (gameStateCopy != null)
            {
                if (IsInstanceValid(gameStateCopy))
                {
                    gameStateCopy.Free();
                }
            }
        }
    }
}
