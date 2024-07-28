using System;
using System.Collections.Generic;
using System.Threading;
using Godot;
using Godot.Collections;
using Mutex = System.Threading.Mutex;

public partial class GameController : Node
{
    // The current state of the game.
    public GameState currentGameState { get; private set; }
    // A copy of the current GameState; This is used when currentGameState is currently working
    public GameState gameStateCopy { get; private set; }
    
    private Mutex taskMutex = new Mutex();
    private Mutex threadMutex = new Mutex();
    private Mutex gameMutex = new Mutex();
    private List<Action> gameTasks = new List<Action>();
    private Thread gameThread;

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

    private void RequestActionAtTask(Vector2I actionLocation, Piece piece)
    {
        // Only use when the game mutex is open
        gameMutex.WaitOne();
        if (piece.teamId != currentGameState.currentPlayerNum)
        {
            return;
        }
        gameMutex.ReleaseMutex();
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
    
    
    
    
    // Function calls to the currentGameState for easier access
    private void NextTask()
    {
        while (true)
        {
            // Close thread mutex before, just in case a task is added here
            threadMutex.WaitOne();
            taskMutex.WaitOne();
            
            // If there are no tasks, stop the thread
            if (gameTasks.Count == 0)
            {
                // Close thread
                gameThread = null;
                taskMutex.ReleaseMutex();
                threadMutex.ReleaseMutex();
                return;
            }
            threadMutex.ReleaseMutex();

            // If there are tasks, run the latest one
            Action taskToDo = gameTasks[0];
            gameTasks.RemoveAt(0);
            taskMutex.ReleaseMutex();

            // GD.Print($"Doing next task: {taskToDo.GetMethodInfo().Name}");
            taskToDo.Invoke();
        }
    }
    
    private void DoTask(Action task)
    {
        // If there's currently a task running, add to tasks and return
        taskMutex.WaitOne();
        gameTasks.Add(task);
        taskMutex.ReleaseMutex();
        threadMutex.WaitOne();
        if (gameThread != null)
        {
            threadMutex.ReleaseMutex();
            return;
        }
        threadMutex.ReleaseMutex();
        
        // If there is no task running, start a new thread
        // Create a new thread, and start it
        gameThread = new Thread(NextTask);
        
        gameThread.Start();
    }

    private void TakeActionAtTask(Vector2I actionLocation, Piece piece)
    {
        gameMutex.WaitOne();
        currentGameState.TakeActionAt(actionLocation, piece);
        gameMutex.ReleaseMutex();
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


    public override void _Notification(int what)
    {
        if (what == NotificationPredelete)
        {
            // On pre-delete, close both Mutex
            taskMutex.Close();
            threadMutex.Close();
            gameMutex.Close();
            gameTasks.Clear();
            if (gameThread != null)
            {
                gameThread.Join();
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
