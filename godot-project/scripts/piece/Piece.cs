using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Piece : GridItem<Piece> {
  [Signal]
  public delegate void InfoChangedEventHandler(PieceInfo info);


  private class ActionsToTake {
    internal int CurActionId;
    internal Array<ActionBase> PossibleActions;

    internal ActionsToTake() {
      PossibleActions = new Array<ActionBase>();
    }

    internal void Add(ActionBase action) {
      PossibleActions.Add(action);
      action.Id = CurActionId++;
    }

    internal void Remove(ActionBase action) {
      PossibleActions.Remove(action);
      // Don't change id, as it may be out of order
    }

    internal void Clear() {
      PossibleActions.Clear();
      CurActionId = 0;
    }
  }


  public PieceDirection ForwardDirection = PieceDirection.Down;

  // Level for this specific piece
  public int Level;

  public Tags Tags = new();
  public int TimesMoved;
  public bool NeedsActionUpdate { get; private set; } = true;

  public PieceInfo Info {
    get => _info;
    internal set {
      // Only send out a signal if the value is different
      if (_info != value) {
        // Enable Action updates, as the info of the piece has changed
        EnableActionsUpdate();
        CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.InfoChanged, value);
      }

      _info = value;
    }
  }

  public int Id { get; internal set; }
  public int LinkId { get; internal set; }

  public Array<ActionBase> CurrentPossibleActions => _actionsToTake.PossibleActions;


  internal bool Taken = false;
  internal int TeamId;


  private PieceInfo _info;

  private readonly ActionsToTake _actionsToTake = new();


  // Called to tell Piece to update actions again
  public void EnableActionsUpdate() {
    NeedsActionUpdate = true;
  }

  public Array<ActionBase> UpdateActions(GameState game) {
    // If info is null, clear actions and ignore
    if (Info == null) {
      _actionsToTake.Clear();
      return CurrentPossibleActions;
    }

    // If the piece isn't on a cell, ignore
    if (Cell == null) {
      return CurrentPossibleActions;
    }

    // If needing to update actions, then update them
    if (NeedsActionUpdate) {
      _actionsToTake.Clear();
      // Loop through the rules and add all the possible actions
      foreach (PieceRule pieceRule in Info.Rules) {
        if (pieceRule.IsEnabled)
          // Get all possible actions for this rule
          // The level is the sum of this piece, the type's level and the rule's level.
          // However, if the rule has "enforceLevel" set to true, only use its level.
        {
          pieceRule.Rule.AddPossibleActions(game, this,
            pieceRule.Level + (pieceRule.EnforceLevel ? 0 : Level + Info.Level));
        }
      }
    }
    else {
      // If an action update is unneeded, just reset the verification
      foreach (ActionBase action in CurrentPossibleActions) {
        action.ResetValidity();
      }
    }

    // Now that actions have been made / reset, validate them
    VerifyMyActions(game);
    // Now that actions have been validated, disable the need for a new
    // update
    NeedsActionUpdate = false;

    return CurrentPossibleActions;
  }

  public Array<ActionBase> GetPossibleActions(GameState game) {
    // Store current needsActions
    bool temp1 = NeedsActionUpdate;
    // Store the current actions, and make a new list
    Array<ActionBase> temp2 = _actionsToTake.PossibleActions;

    // Set the values to their defaults for making new actions
    NeedsActionUpdate = true;
    _actionsToTake.PossibleActions = new Array<ActionBase>();

    // Update the list and store it
    Array<ActionBase> returnArray = UpdateActions(game);

    // Return the values to what they were before
    NeedsActionUpdate = temp1;
    _actionsToTake.PossibleActions = temp2;

    return returnArray;
  }

  public void AddAction(ActionBase action) {
    _actionsToTake.Add(action);
  }

  public void RemoveAction(ActionBase action) {
    _actionsToTake.Remove(action);
  }

  public Array<ActionBase> ValidateActions(GameState game, Array<ActionBase> actions) {
    var validActions = new Array<ActionBase>();

    foreach (ActionBase action in actions)
    foreach (ValidationRuleBase rule in Info.ValidationRules) {
      rule.CheckAction(game, this, action);
    }

    // Check for invalid tags afterwards
    foreach (ActionBase action in actions)
      // If there are no invalid tags, and the action is valid, then it's valid.
    {
      if (action.InvalidTags.Count == 0) {
        if (action.Valid) {
          validActions.Add(action);
        }
      }
      else {
        action.MakeInvalid();
      }
    }

    return validActions;
  }

  public void ClearActions() {
    // If it's null, it's already cleared
    if (CurrentPossibleActions == null) {
      return;
    }

    // Go through all actions and free them
    foreach (ActionBase action in CurrentPossibleActions) {
      if (IsInstanceValid(action)) {
        // Only continue if this piece (by reference) owns the action.
        if (action.Owner != this) {
          continue;
        }

        action.CallDeferred(Node.MethodName.QueueFree);

        if (action.Cell != null) {
          action.Cell.RemoveItem(action);
        }
      }
    }

    // Clear the list
    _actionsToTake.Clear();
  }

  public void NewTurn(GameState game) {
    if (Info == null) {
      return;
    }

    foreach (PieceRule pieceRule in Info.Rules) {
      pieceRule.Rule.NewTurn(game, this);
    }
  }

  public void EndTurn(GameState game) {
    if (Info == null) {
      return;
    }

    foreach (PieceRule pieceRule in Info.Rules) {
      pieceRule.Rule.EndTurn(game, this);
    }
  }

  public string GetPieceInfoId() {
    return Info?.PieceId;
  }

  public bool IsPiece(string id) {
    if (Info == null) {
      return false;
    }

    return Info.PieceId == id;
  }

  public bool HasTag(string tag) {
    return Tags.Contains(tag);
  }

  public Piece Clone() {
    var newPiece = new Piece();
    // Copy variables
    newPiece._info = Info;
    newPiece.Id = Id;
    newPiece.LinkId = LinkId;
    newPiece.TeamId = TeamId;

    newPiece.ForwardDirection = ForwardDirection;

    newPiece.TimesMoved = TimesMoved;
    newPiece.NeedsActionUpdate = NeedsActionUpdate;

    foreach (string tag in Tags) {
      newPiece.Tags.Add(tag);
    }

    return newPiece;
  }


  internal void SetInfoWithoutSignal(PieceInfo newInfo) {
    _info = newInfo;
  }

  internal Array<ActionBase> VerifyMyActions(GameState game) {
    return ValidateActions(game, _actionsToTake.PossibleActions);
  }
}