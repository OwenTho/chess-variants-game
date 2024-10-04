using System;

public class EventListener {
  // When calling event listeners, it will only call if this returns true.
  public Func<bool> EnabledFunction;

  // When null, defaults to EventResult.Continue
  public Func<GameState, EventResult> FlagFunction;

  // Function called when the event occurs
  internal Action<GameState> Listen;

  public EventListener(string eventId, Action<GameState> listen) {
    EventId = eventId;
    Listen = listen;
  }

  public EventListener(string eventId, Action<GameState> listen,
    Func<GameState, EventResult> flagFunction) : this(
    eventId, listen) {
    FlagFunction = flagFunction;
  }

  public EventListener(string eventId, Action<GameState> listen, Func<bool> enabledFunction) : this(
    eventId, listen) {
    EnabledFunction = enabledFunction;
  }

  public EventListener(string eventId, Action<GameState> listen,
    Func<GameState, EventResult> flagFunction,
    Func<bool> enabledFunction) : this(
    eventId, listen, flagFunction) {
    EnabledFunction = enabledFunction;
  }

  // The id of the event being listened for
  public string EventId { get; private set; }
}