using System;

public class EventListener
{
    // The id of the event being listened for
    public string eventId { get; private set; }
    
    // Function called when the event occurs
    internal Action<GameState> listen;
    
    // When null, defaults to EventResult.Continue
    public Func<GameState, EventResult> flagFunction;

    // When calling event listeners, it will only call if this returns true.
    public Func<bool> enabledFunction;

    public EventListener(string eventId, Action<GameState> listen)
    {
        this.eventId = eventId;
        this.listen = listen;
    }

    public EventListener(string eventId,Action<GameState> listen, Func<GameState, EventResult> flagFunction) : this(eventId, listen)
    {
        this.flagFunction = flagFunction;
    }

    public EventListener(string eventId, Action<GameState> listen, Func<bool> enabledFunction) : this(eventId, listen)
    {
        this.enabledFunction = enabledFunction;
    }

    public EventListener(string eventId, Action<GameState> listen, Func<GameState, EventResult> flagFunction, Func<bool> enabledFunction) : this(
        eventId, listen, flagFunction)
    {
        this.enabledFunction = enabledFunction;
    }
}
