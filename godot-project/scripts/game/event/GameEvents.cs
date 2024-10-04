using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public partial class GameEvents : Node {
  [Signal]
  public delegate void WaitEndedEventHandler();

  [Signal]
  public delegate void WaitStartedEventHandler();

  public bool CanWait;
  private EventListener _currentListener;
  public bool DoWait;
  private readonly Dictionary<string, List<EventListener>> _eventListeners = new();
  private Mutex _processMutex = new();
  private bool _waiting;
  private Mutex _waitingMutex = new();

  public GameEvents(GameState game) {
    Game = game;
  }

  public GameEvents(GameState game, bool doWait) : this(game) {
    DoWait = doWait;
    CanWait = doWait;
  }

  public GameEvents(GameState game, bool doWait, bool canWait) : this(game, doWait) {
    CanWait = canWait;
  }

  public GameState Game { get; }

  public void AddListener(EventListener listener) {
    // If the list can't be retrieved, make a new one
    if (!_eventListeners.TryGetValue(listener.EventId, out List<EventListener> listeners)) {
      listeners = new List<EventListener>();
      _eventListeners.Add(listener.EventId, listeners);
    }

    listeners.Add(listener);
  }

  /// <summary>
  ///   Announce the event to listeners listing for a certain event.
  /// </summary>
  /// <param name="eventKey">String name of the event.</param>
  /// <returns name="stopEvent">Boolean. True if the event should occur, False if it should not.</returns>
  public bool AnnounceEvent(string eventKey) {
    _processMutex.Lock();
    _waitingMutex.Lock();
    bool isWaiting = _waiting;
    _waitingMutex.Unlock();
    if (isWaiting) {
      GD.PushError("Can't announce event as there is one currently waiting.");
      _processMutex.Unlock();
      return true;
    }

    // Get the list of listeners. If there isn't one, just ignore
    if (!_eventListeners.TryGetValue(eventKey, out List<EventListener> listeners)) {
      _processMutex.Unlock();
      return true;
    }

    // First get if the event result
    var waitingListeners = new List<EventListener>();
    var result = EventResult.Continue;
    foreach (EventListener listener in listeners) {
      _currentListener = listener;
      // If the listener doesn't have a flag function, skip it
      if (listener.FlagFunction == null) {
        continue;
      }

      // If the listener is disabled, skip it
      if (listener.EnabledFunction != null && !listener.EnabledFunction()) {
        continue;
      }

      EventResult listenerResult = listener.FlagFunction(Game);
      // If it doesn't continue, remove the continue flag
      if (listenerResult.HasFlag(EventResult.Continue)) {
        result &= ~EventResult.Continue;
      }

      if (CanWait) {
        if (listenerResult.HasFlag(EventResult.Wait) && DoWait) {
          waitingListeners.Add(listener);
        }
        else
          // Remove the wait if it should not be there.
        {
          listenerResult &= ~EventResult.Wait;
        }
      }
      else {
        GD.PushError("A listener provided a Wait flag despite GameEvents having it disabled.");
      }

      // If it has the Wait tag, push an error if the GameEvents can't wait
      if (!DoWait && listenerResult.HasFlag(EventResult.Wait)) {
      }

      // Remove the Continue tag from the listener result
      listenerResult &= ~EventResult.Continue;

      // OR the rest of the results
      result |= listenerResult;
    }

    _currentListener = null;

    // If the event is cancelled, return
    if (result.HasFlag(EventResult.Cancel)) {
      _processMutex.Unlock();
      return false;
    }

    // Now do the events
    foreach (EventListener listener in listeners) {
      _currentListener = listener;
      if (listener.EnabledFunction == null || listener.EnabledFunction()) {
        listener.Listen(Game);
      }

      // Wait if a listener needs to do something following the listen
      if (CanWait && DoWait && result.HasFlag(EventResult.Wait) &&
          waitingListeners.Contains(listener)) {
        Task waitTask = Wait();
        waitTask.Wait();
      }
    }

    _currentListener = null;

    // If result does not continue, stop the event
    if (!result.HasFlag(EventResult.Continue)) {
      _processMutex.Unlock();
      return false;
    }

    _processMutex.Unlock();
    return true;
  }

  internal async Task Wait() {
    _waitingMutex.Lock();
    CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.WaitStarted);
    _waiting = true;
    bool stillWaiting = true;
    _waitingMutex.Unlock();
    while (stillWaiting) {
      await Task.Delay(25);
      _waitingMutex.Lock();
      stillWaiting = _waiting;
      _waitingMutex.Unlock();
    }
  }

  public void EndWait() {
    _waitingMutex.Lock();
    if (_waiting) {
      CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.WaitEnded);
    }

    _waiting = false;
    _waitingMutex.Unlock();
  }
}