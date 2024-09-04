using System.Collections.Generic;
using Godot;

public partial class GameEvents
{
    public GameState game { get; }
    private bool canWait = false;
    private EventListener currentListener;
    private Dictionary<string, List<EventListener>> eventListeners = new Dictionary<string, List<EventListener>>();
    private Mutex waitingMutex = new();
    private bool waiting = false;

    public GameEvents(GameState game)
    {
        this.game = game;
    }
    
    public GameEvents(GameState game, bool canWait) : this(game)
    {
        this.canWait = canWait;
    }
    
    public void AddListener(EventListener listener)
    {
        // If the list can't be retrieved, make a new one
        if (!eventListeners.TryGetValue(listener.eventId, out List<EventListener> listeners))
        {
            listeners = new List<EventListener>();
            eventListeners.Add(listener.eventId, listeners);
        }

        listeners.Add(listener);
    }
        
    /// <summary>
    /// Announce the event to listeners listing for a certain event.
    /// </summary>
    /// <param name="eventKey">String name of the event.</param>
    /// <returns name="stopEvent">Boolean. True if the event should occur, False if it should not.</returns>
    internal bool AnnounceEvent(string eventKey)
    {
        waitingMutex.Lock();
        bool isWaiting = waiting;
        waitingMutex.Unlock();
        if (isWaiting)
        {
            GD.PushError("Can't announce event as there is one currently waiting.");
            return true;
        }
        // Get the list of listeners. If there isn't one, just ignore
        if (!eventListeners.TryGetValue(eventKey, out List<EventListener> listeners))
        {
            return true;
        }
        
        // First get if the event result
        EventResult result = EventResult.Continue;
        foreach (EventListener listener in listeners)
        {
            currentListener = listener;
            // If the listener doesn't have a flag function, skip it
            if (listener.flagFunction == null)
            {
                continue;
            }
            // If the listener is disabled, skip it
            if (listener.enabledFunction != null && !listener.enabledFunction())
            {
                continue;
            }

            EventResult listenerResult = listener.flagFunction(game);
            // If it doesn't continue, remove the continue flag
            if (listenerResult.HasFlag(EventResult.Continue))
            {
                result &= ~EventResult.Continue;
            }
            // If it has the Wait tag, push an error if the GameEvents can't wait
            if (listenerResult.HasFlag(EventResult.Wait) && !canWait)
            {
                GD.PushError("A listener provided a Wait flag despite GameEvents having it disabled.");
            }
            // Remove the Continue tag from the listener result
            listenerResult &= ~EventResult.Continue;
            
            // OR the rest of the results
            result |= listenerResult;
        }

        currentListener = null;
        
        // If the event is cancelled, return
        if (result.HasFlag(EventResult.Cancel))
        {
            return false;
        }
        
        // Now do the events
        foreach (EventListener listener in listeners)
        {
            currentListener = listener;
            if (listener.enabledFunction == null || listener.enabledFunction())
            {
                listener.listen(game);
            }
        
            // Wait if a listener needs to do something following the listen
            if (result.HasFlag(EventResult.Wait) && canWait)
            {
                waitingMutex.Lock();
                waiting = true;
                waitingMutex.Unlock();
                while (waiting)
                {
                    
                }
            }
        }

        currentListener = null;
        
        // If result does not continue, stop the event
        if (!result.HasFlag(EventResult.Continue))
        {
            return false;
        }

        return true;
    }

    public void EndWait()
    {
        waitingMutex.Lock();
        waiting = false;
        waitingMutex.Unlock();
    }
}
