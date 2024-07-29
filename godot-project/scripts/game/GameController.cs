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
    public Semaphore threadSemaphore = new Semaphore();
    public Mutex taskMutex = new Mutex();
    public Mutex threadMutex = new Mutex();
    public Mutex gameMutex = new Mutex();
    private List<Action> gameTasks = new List<Action>();
    private GodotThread gameThread;

    public const int NUMBER_OF_PLAYERS = 2;
    
    // This will likely be fine, as the grid size will not change during play.
    public Vector2I gridSize => currentGameState.gridSize;
    
    // Grid is also unchanging, only changing its contents. Mutex only has to be
    // used as needed when altering / using contents.
    public Grid<GameItem> grid => currentGameState.grid;

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

    public bool TryGetPieceInfo(string key, out PieceInfo info)
    {
        info = GetPieceInfo(key);
        return info != null;
    }

    public RuleBase GetRule(string key)
    {
        return actionRuleRegistry.GetValue(key);
    }





    private void StartGameTask()
    {
        gameMutex.Lock();
        currentGameState.StartGame();
        gameMutex.Unlock();
    }

    public void StartGame()
    {
        DoTask(StartGameTask);
    }

    private int GetCurrentPlayerTask()
    {
        gameMutex.Lock();
        int curPlayer = currentGameState.currentPlayerNum;
        gameMutex.Unlock();
        return curPlayer;
    }

    public int GetCurrentPlayer()
    {
        gameMutex.Lock();
        int curPlayer = currentGameState.currentPlayerNum;
        gameMutex.Unlock();
        return curPlayer;
    }

    
    private Piece PlacePiece(string pieceId, int linkId, int teamId, int x, int y, int id = -1)
    {
        gameMutex.Lock();
        Piece returnPiece = currentGameState.PlacePiece(pieceId, linkId, teamId, x, y, id);
        gameMutex.Unlock();
        return returnPiece;
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
    
    
    
    private void RequestActionAtTask(Vector2I actionLocation, Piece piece)
    {
        // Only use when the game mutex is open
        gameMutex.Lock();
        if (piece.teamId != currentGameState.currentPlayerNum)
        {
            return;
        }
        gameMutex.Unlock();
        CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.RequestedActionAt, actionLocation, piece);
    }

    public void RequestAction(ActionBase action, Piece piece)
    {
        // Request at the location
        RequestActionsAt(action.actionLocation, piece);
    }

    public void RequestActionsAt(Vector2I actionLocation, Piece piece)
    {
        // Tell networking "I want to act this piece on this location"
        DoTask(() => RequestActionAtTask(actionLocation, piece));
    }
    
    
    

    private void TakeActionAtTask(Vector2I actionLocation, Piece piece)
    {
        gameMutex.Lock();
        currentGameState.TakeActionAt(actionLocation, piece);
        gameMutex.Unlock();
    }
    
    public void TakeAction(ActionBase action, Piece piece)
    {
        if (action == null)
        {
            return;
        }
        TakeActionAt(action.actionLocation, piece);
    }

    public void TakeActionAt(Vector2I actionLocation, Piece piece)
    {
        DoTask(() => TakeActionAtTask(actionLocation, piece));
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
