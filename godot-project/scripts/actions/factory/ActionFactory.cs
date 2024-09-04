
using Godot;
using Godot.Collections;

public abstract partial class ActionFactory : Node 
{
    public string actionId { get; internal set; }

    internal ActionBase CreateNewActionFromDict(Dictionary<string, string> actionDict)
    {
        ActionBase newAction = CreateAction();
        newAction.FromDict(actionDict);
        return newAction;
    }

    public abstract ActionBase CreateAction();

    public abstract bool ActionTypeMatches(ActionBase action);
}