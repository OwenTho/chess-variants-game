using Godot;
using Godot.Collections;

public abstract partial class ActionFactory : Node {
  public string ActionId { get; internal set; }

  public abstract ActionBase CreateAction();

  public abstract bool ActionTypeMatches(ActionBase action);


  internal ActionBase CreateNewActionFromDict(Dictionary<string, string> actionDict) {
    ActionBase newAction = CreateAction();
    newAction.FromDict(actionDict);
    return newAction;
  }
}