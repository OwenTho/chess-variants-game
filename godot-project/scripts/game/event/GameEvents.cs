using System.Collections.Generic;

public partial class GameEvents
{
    private GameState game;
    private Dictionary<string, List<EventListener>> eventListeners = new Dictionary<string, List<EventListener>>();

    public GameEvents(GameState game)
    {
        this.game = game;
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
        // Get the list of listeners. If there isn't one, just ignore
        if (!eventListeners.TryGetValue(eventKey, out List<EventListener> listeners))
        {
            return true;
        }
        
        // First get if the event result
        EventResult result = EventResult.Continue;
        foreach (EventListener listener in listeners)
        {
            if (listener.flagFunction == null)
            {
                continue;
            }

            EventResult listenerResult = listener.flagFunction(game);
            // If it doesn't continue, remove the continue flag
            if (listenerResult.HasFlag(EventResult.Continue))
            {
                result &= ~EventResult.Continue;
            }
            // Remove the Continue tag from the listener result
            listenerResult &= ~EventResult.Continue;
            
            // OR the rest of the results
            result |= listenerResult;
        }
        
        // If the event is cancelled, return
        if (result.HasFlag(EventResult.Cancel))
        {
            return false;
        }
        
        // Now do the events
        foreach (EventListener listener in listeners)
        {
            listener.listen(game);
        }
        
        // Wait if a listener needs to do something following the listen
        if (result.HasFlag(EventResult.Wait))
        {
            // TODO: Add the ability for Cards to pause the game to do things
        }
        
        // If result does not continue, stop the event
        if (!result.HasFlag(EventResult.Continue))
        {
            return false;
        }

        return true;
    }
}
