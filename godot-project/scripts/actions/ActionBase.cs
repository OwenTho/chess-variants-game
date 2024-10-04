using System.Collections.Generic;
using Godot;

[GlobalClass]
public abstract partial class ActionBase : GridItem<ActionBase> {
  public enum CarryType {
    None,
    Down,
    Up
  }

  public Vector2I ActionLocation;
  public int Id = -1;

  // A stored validation, for use with Check checks.
  internal bool StoredValidation = true;

  public ActionBase() {
  }

  public ActionBase(Piece owner, Vector2I actionLocation) {
    ActionLocation = actionLocation;
    Owner = owner;

    Dependencies = new List<ActionBase>();
    Dependents = new List<ActionBase>();
  }

  public new Piece Owner { get; private set; }

  // If the action does something or not. Actions for checking validating (e.g: Sliding) set this to false.
  public bool Acting { get; protected set; } = true;

  // If the action is valid and can be done
  public bool Valid { get; private set; } = true;

  // Rules that this rule depends on. If these are invalid, this one should
  // also be invalid.
  public List<ActionBase> Dependencies { get; }

  // Rules that depend on this one.
  public List<ActionBase> Dependents { get; }

  // Extra tags of the Action. Add to this in the Move Rules.
  public Tags Tags { get; } = new();

  // Tags used in validation steps. Add this in Verification Rules.
  public CountingTags VerifyTags { get; } = new();

  // Tags that make this action invalid. Add to this in Verification Rules.
  public CountingTags InvalidTags { get; } = new();

  public void MakeDependentsInvalid() {
    foreach (ActionBase dependent in Dependents) {
      dependent.MakeInvalid();
    }
  }

  public void MakeDependenciesInvalid() {
    foreach (ActionBase dependency in Dependencies) {
      dependency.MakeInvalid(CarryType.Up);
    }
  }

  public void MakeInvalid(CarryType carryType = CarryType.Down) {
    // If already invalid, ignore
    if (!Valid) {
      return;
    }

    // Make invalid, and pass to dependents
    Valid = false;
    if (carryType == CarryType.Down) {
      MakeDependentsInvalid();
    }

    if (carryType == CarryType.Up) {
      MakeDependenciesInvalid();
    }
  }

  // Find if this action has a certain dependency.
  // It checks all dependencies, to make sure.
  public bool HasDependency(ActionBase action) {
    if (Dependencies.Contains(action)) {
      return true;
    }

    // If not a dependency here, check dependencies
    foreach (ActionBase dependency in Dependencies) {
      if (dependency.HasDependency(action)) {
        return true;
      }
    }

    return false;
  }

  public void AddDependency(ActionBase action) {
    if (action == this) {
      GD.PushError("Tried to add an Action as a dependency of itself.");
      return;
    }

    if (action == null || Dependencies.Contains(action)) {
      return;
    }

    // Make sure it's not a dependent
    if (HasDependent(action)) {
      GD.PushError($"Tried to make {this} a dependent of {action}, when it is a dependency.");
      return;
    }

    Dependencies.Add(action);
    action.AddDependent(this);
  }

  // Find if this action has a certain dependent.
  // It checks all dependents, to make sure.
  public bool HasDependent(ActionBase action) {
    if (Dependencies.Contains(action)) {
      return true;
    }

    // If not a dependent here, check dependencies
    foreach (ActionBase dependent in Dependents) {
      if (dependent.HasDependent(action)) {
        return true;
      }
    }

    return false;
  }

  public void AddDependent(ActionBase action) {
    if (action == this) {
      GD.PushError("Tried to add an Action as a dependent of itself.");
      return;
    }

    if (action == null || Dependents.Contains(action)) {
      return;
    }

    // Make sure it's not a dependency
    if (HasDependency(action)) {
      GD.PushError($"Tried to make {this} a dependency of {action}, when it is a dependent.");
      return;
    }

    Dependents.Add(action);
    action.AddDependency(this);
  }

  public void Tag(string tag, CarryType carryType = CarryType.Down) {
    // If this already has the tag, ignore to avoid
    // infinite loops.
    if (Tags.Contains(tag)) {
      return;
    }

    Tags.Add(tag);
    // If carry, also tag dependents
    if (carryType == CarryType.Down) {
      TagDependents(tag);
    }

    if (carryType == CarryType.Up) {
      TagDependencies(tag);
    }
  }

  public void TagDependents(string tag) {
    foreach (ActionBase dependent in Dependents) {
      dependent.Tag(tag);
    }
  }

  public void TagDependencies(string tag) {
    foreach (ActionBase dependency in Dependencies) {
      dependency.Tag(tag, CarryType.Up);
    }
  }

  public void VerifyTag(string tag, CarryType carryType = CarryType.Down, int tagCount = 1) {
    // If this already has the tag, ignore to avoid
    // infinite loops.
    if (VerifyTags.Contains(tag)) {
      return;
    }

    VerifyTags.Add(tag);
    // If carry, also tag dependents
    if (carryType == CarryType.Down) {
      VerifyTagDependents(tag, tagCount);
    }

    if (carryType == CarryType.Up) {
      VerifyTagDependencies(tag, tagCount);
    }
  }

  public void VerifyTagDependents(string tag, int tagCount = 1) {
    foreach (ActionBase dependent in Dependents) {
      dependent.VerifyTag(tag, CarryType.Down, tagCount);
    }
  }

  public void VerifyTagDependencies(string tag, int tagCount = 1) {
    foreach (ActionBase dependency in Dependencies) {
      dependency.VerifyTag(tag, CarryType.Up, tagCount);
    }
  }

  public void RemoveVerifyTag(string tag, CarryType carryType = CarryType.Down, int tagCount = 1) {
    VerifyTag(tag, carryType, -tagCount);
  }

  public void RemoveVerifyTagDependents(string tag, int tagCount = 1) {
    VerifyTagDependents(tag, -tagCount);
  }

  public void RemoveVerifyTagDependencies(string tag, int tagCount = 1) {
    VerifyTagDependencies(tag, -tagCount);
  }


  public void InvalidTag(string tag, CarryType carryType = CarryType.Down, int tagCount = 1) {
    InvalidTags.Add(tag, tagCount);
    // If carry, also tag dependents
    if (carryType == CarryType.Down) {
      InvalidTagDependents(tag, tagCount);
    }

    if (carryType == CarryType.Up) {
      InvalidTagDependencies(tag, tagCount);
    }
  }

  public void InvalidTagDependents(string tag, int tagCount = 1) {
    foreach (ActionBase dependent in Dependents) {
      dependent.InvalidTag(tag, CarryType.Down, tagCount);
    }
  }

  public void InvalidTagDependencies(string tag, int tagCount = 1) {
    foreach (ActionBase dependency in Dependencies) {
      dependency.InvalidTag(tag, CarryType.Up, tagCount);
    }
  }

  public void RemoveInvalidTag(string tag, CarryType carryType = CarryType.Down, int tagCount = 1) {
    InvalidTag(tag, carryType, -tagCount);
  }

  public void RemoveInvalidTagDependents(string tag, int tagCount = 1) {
    InvalidTagDependents(tag, -tagCount);
  }

  public void RemoveInvalidTagDependencies(string tag, int tagCount = 1) {
    InvalidTagDependents(tag, -tagCount);
  }

  public void SetOwner(Piece newOwner, CarryType carryType = CarryType.Down) {
    // If owner is the same, ignore
    if (Owner == newOwner) {
      return;
    }

    // If old owner is not null, remove this from the owner
    if (Owner != null) {
      Owner.RemoveAction(this);
    }

    Owner = newOwner;

    if (carryType == CarryType.Down) {
      foreach (ActionBase dependent in Dependents) {
        dependent.SetOwner(newOwner);
      }
    }

    if (carryType == CarryType.Up) {
      foreach (ActionBase dependency in Dependencies) {
        dependency.SetOwner(newOwner, CarryType.Up);
      }
    }

    // Assume the owner already has the action
  }

  public ActionBase CloneAction() {
    ActionBase clonedAction = Clone();
    CloneTo(clonedAction);
    return clonedAction;
  }

  public void AddVector2IToDict(string name, Vector2I location,
    Godot.Collections.Dictionary<string, string> actionDict) {
    actionDict.Add($"{name}_x", location.X.ToString());
    actionDict.Add($"{name}_y", location.Y.ToString());
  }

  public Vector2I ReadVector2IFromDict(string name,
    Godot.Collections.Dictionary<string, string> actionDict) {
    var location = new Vector2I();
    if (actionDict.TryGetValue($"{name}_x", out string dictLocX)) {
      if (int.TryParse(dictLocX, out int locX)) {
        location.X = locX;
      }
      else {
        GD.PushError($"{name}_x is not a number.");
      }
    }
    else {
      GD.PushError($"{name}_x not found in dictionary.");
    }


    if (actionDict.TryGetValue($"{name}_y", out string dictMoveLocY)) {
      if (int.TryParse(dictMoveLocY, out int locY)) {
        location.Y = locY;
      }
      else {
        GD.PushError($"{name}_y is not a number.");
      }
    }
    else {
      GD.PushError($"{name}_y not found in dictionary.");
    }

    return location;
  }


  public virtual Godot.Collections.Dictionary<string, string> ToDict() {
    return new Godot.Collections.Dictionary<string, string>();
  }

  public abstract void ActOn(GameState game, Piece piece);

  public abstract void FromDict(Godot.Collections.Dictionary<string, string> actionDict);


  public override string ToString() {
    return GetType().Name;
  }


  internal void ResetValidity() {
    // Set to be valid again
    Valid = true;
    // Clear tags
    VerifyTags.Clear();
    InvalidTags.Clear();
  }

  internal void SetValidToStored() {
    Valid = StoredValidation;
  }

  internal Godot.Collections.Dictionary<string, string> ConvertToDict(GameState game) {
    Godot.Collections.Dictionary<string, string> actionData = ToDict();
    // Remove action_id from the dictionary, and force the correct one
    actionData.Remove("action_id");
    actionData.Add("action_id", game.GetActionId(this));
    return actionData;
  }

  protected abstract ActionBase Clone();

  private void CloneTo(ActionBase action) {
    // First clear the action
    action.VerifyTags.Clear();
    action.InvalidTags.Clear();
    action.Tags.Clear();

    // Then copy over the tags
    foreach (string tag in Tags) {
      action.Tags.Add(tag);
    }

    foreach (string tag in VerifyTags)
      // Get the tag counts, and add it that number of times
    {
      if (VerifyTags.TryGetTagCount(tag, out int tagCount)) {
        action.VerifyTags.Add(tag, tagCount);
      }
    }

    foreach (string tag in InvalidTags)
      // Get the tag counts, and add it that number of times
    {
      if (InvalidTags.TryGetTagCount(tag, out int tagCount)) {
        action.InvalidTags.Add(tag, tagCount);
      }
    }

    // Copy over the other variables
    // Don't copy owner, as it would have the copy owner too
    action.Id = Id;
    action.Valid = Valid;
    action.ActionLocation = ActionLocation;
  }
}