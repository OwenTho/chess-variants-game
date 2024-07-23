using Godot;
using Godot.Collections;
using System.Collections.Generic;

public partial class Piece : GridItem
{
    public int timesMoved = 0;
    internal int teamId = 0;

    private PieceInfo _info;
    public PieceInfo info {
        get
        {
            return _info;
        }
        internal set
        {
            _info = value;
            EmitSignal(SignalName.InfoChanged, value);
        }
    }
    public int id { get; internal set; }
    public int linkId { get; internal set; }
    public Vector2I forwardDirection = Vector2I.Down;

    public Tags tags = new Tags();


    [Signal]
    public delegate void InfoChangedEventHandler(PieceInfo info);

    private class ActionsToTake
    {
        internal Array<ActionBase> possibleActions;
        internal ActionsToTake()
        {
            possibleActions = new Array<ActionBase>();
        }

        internal void Add(ActionBase action)
        {
            possibleActions.Add(action);
        }

        internal void Clear()
        {
            possibleActions.Clear();
        }
    }

    private ActionsToTake actionsToTake = new ActionsToTake();

    public Array<ActionBase> currentPossibleActions
    {
        get { return actionsToTake.possibleActions; }
    }

    public Array<ActionBase> UpdateActions(GameController game)
    {
        actionsToTake.Clear();
        // Loop through the rules and add all of the possible actions
        foreach (PieceRule pieceRule in info.rules)
        {
            if (pieceRule.isEnabled)
            {
                pieceRule.rule.AddPossibleActions(game, this);
            }
        }
        // Validate all actions
        VerifyMyActions(game);
        return actionsToTake.possibleActions;
    }

    public Array<ActionBase> GetPossibleActions(GameController game)
    {
        // Store the current actions, and make a new list
        Array<ActionBase> temp = actionsToTake.possibleActions;
        actionsToTake.possibleActions = new Array<ActionBase>();
        // Update the list and store it, returning the new values
        Array<ActionBase> returnArray = UpdateActions(game);
        actionsToTake.possibleActions = temp;
        return returnArray;
    }

    public void AddAction(ActionBase action)
    {
        actionsToTake.Add(action);
    }

    private Array<ActionBase> VerifyMyActions(GameController game)
    {
        return ValidateActions(game, actionsToTake.possibleActions);
    }

    public Array<ActionBase> ValidateActions(GameController game, Array<ActionBase> actions)
    {
        Array<ActionBase> validActions = new Array<ActionBase>();

        foreach (ActionBase action in actions)
        {
            foreach (ValidationRuleBase rule in info.validationRules)
            {
                rule.CheckAction(game, this, action);
            }
        }

        // Check for invalid tags afterwards
        foreach (ActionBase action in actions)
        {
            // If there are no invalid tags, and the action is valid, then it's valid.
            if (action.invalidTags.Count == 0)
            {
                if (action.valid)
                {
                    validActions.Add(action);
                }
            }
            else
            {
                action.MakeInvalid();
            }
        }

        return validActions;
    }

    public void ClearActions()
    {
        // If it's null, it's already cleared
        if (currentPossibleActions == null)
        {
            return;
        }
        // Go through all actions and free them
        foreach (var action in currentPossibleActions)
        {
            action.QueueFree();
        }
        // Clear the list
        actionsToTake.Clear();
    }

    public void NewTurn(GameController game)
    {
        if (info == null)
        {
            return;
        }
        foreach (PieceRule pieceRule in info.rules)
        {
            pieceRule.rule.NewTurn(game, this);
        }
    }

    public void EndTurn(GameController game)
    {
        if (info == null)
        {
            return;
        }
        foreach (PieceRule pieceRule in info.rules)
        {
            pieceRule.rule.EndTurn(game, this);
        }
    }
}
