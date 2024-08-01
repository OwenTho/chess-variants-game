using System;
using Godot;
using Godot.Collections;

public partial class Piece : GameItem
{
    public int timesMoved = 0;
    internal int teamId = 0;
    public bool needsActionUpdate { get; private set; } = true;

    private PieceInfo _info;
    public PieceInfo info {
        get
        {
            return _info;
        }
        internal set
        {
            _info = value;
            CallDeferred(GodotObject.MethodName.EmitSignal, SignalName.InfoChanged, value);
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
        internal int curActionId;
        internal ActionsToTake()
        {
            possibleActions = new Array<ActionBase>();
        }

        internal void Add(ActionBase action)
        {
            possibleActions.Add(action);
            action.actionId = curActionId++;
        }

        internal void Remove(ActionBase action)
        {
            possibleActions.Remove(action);
            // Don't change id, as it may be out of order
        }

        internal void Clear()
        {
            possibleActions.Clear();
            curActionId = 0;
        }
    }

    private ActionsToTake actionsToTake = new ActionsToTake();

    public Array<ActionBase> currentPossibleActions { get { return actionsToTake.possibleActions; } }

    // Called to tell Piece to update actions again
    public void EnableActionsUpdate()
    {
        needsActionUpdate = true;
    }
    
    public Array<ActionBase> UpdateActions(GameState game)
    {
        // If the piece isn't on a cell, ignore
        if (cell == null)
        {
            return currentPossibleActions;
        }
        // If needing to update actions, then update them
        if (needsActionUpdate)
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
        }
        else
        {
            // If an action update is unneeded, just reset the verification
            foreach (var action in currentPossibleActions)
            {
                action.ResetValidity();
            }
        }
        // Now that actions have been made / reset, validate them
        VerifyMyActions(game);
        // Now that actions have been validated, disable the need for a new
        // update
        needsActionUpdate = false;

        return currentPossibleActions;
    }

    public Array<ActionBase> GetPossibleActions(GameState game)
    {
        // Store current needsActions
        bool temp1 = needsActionUpdate;
        // Store the current actions, and make a new list
        Array<ActionBase> temp2 = actionsToTake.possibleActions;
        
        // Set the values to their defaults for making new actions
        needsActionUpdate = true;
        actionsToTake.possibleActions = new Array<ActionBase>();
        
        // Update the list and store it
        Array<ActionBase> returnArray = UpdateActions(game);
        
        // Return the values to what they were before
        needsActionUpdate = temp1;
        actionsToTake.possibleActions = temp2;
        
        return returnArray;
    }

    public void AddAction(ActionBase action)
    {
        actionsToTake.Add(action);
    }

    public void RemoveAction(ActionBase action)
    {
        actionsToTake.Remove(action);
    }

    private Array<ActionBase> VerifyMyActions(GameState game)
    {
        return ValidateActions(game, actionsToTake.possibleActions);
    }

    public Array<ActionBase> ValidateActions(GameState game, Array<ActionBase> actions)
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
            if (IsInstanceValid(action))
            {
                action.QueueFree();
                if (action.cell != null)
                {
                    action.cell.RemoveItem(action);
                }
            }
        }
        // Clear the list
        actionsToTake.Clear();
    }

    public void NewTurn(GameState game)
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

    public void EndTurn(GameState game)
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



    public bool IsPiece(string id)
    {
        if (info == null)
        {
            return false;
        }

        return info.pieceId == id;
    }
    

    public override Piece Clone()
    {
        Piece newPiece = new Piece();
        // Copy variables
        newPiece._info = info;
        newPiece.id = id;
        newPiece.linkId = linkId;
        newPiece.teamId = teamId;
        
        newPiece.forwardDirection = forwardDirection;
        
        newPiece.timesMoved = timesMoved;
        newPiece.needsActionUpdate = needsActionUpdate;

        foreach (var tag in tags)
        {
            newPiece.tags.Add(tag);
        }

        return newPiece;
    }
}
