using System;

public class EventListener
{
    // The id of the event being listened for
    public string eventId { get; private set; }
    
    // Function called when the event occurs
    internal Action<GameState> listen;
    
    // When null, defaults to EventResult.Continue
    public Func<GameState, EventResult> flagFunction;

    public EventListener(string eventId, Action<GameState> listen)
    {
        this.eventId = eventId;
        this.listen = listen;
    }

    public EventListener(string eventId,Action<GameState> listen, Func<GameState, EventResult> flagFunction) : this(eventId, listen)
    {
        this.flagFunction = flagFunction;
    }
}
