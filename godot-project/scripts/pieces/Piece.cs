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

    public Array<ActionBase> currentPossibleActions { get; internal set; }

    [Signal]
    public delegate void InfoChangedEventHandler(PieceInfo info);

    public Array<ActionBase> GetPossibleActions(GameController game)
    {
        Array<ActionBase> allPossibleActions = new Array<ActionBase>();
        foreach (PieceRule pieceRule in info.rules)
        {
            if (pieceRule.isEnabled)
            {
                Array<ActionBase> possibleActions = pieceRule.rule.GetPossibleActions(game, this);

                ValidateActions(game, possibleActions);
                allPossibleActions.AddRange(possibleActions);
            }
        }
        return allPossibleActions;
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

    public Array<ActionBase> UpdateActions(GameController game)
    {
        currentPossibleActions = GetPossibleActions(game);
        return currentPossibleActions;
    }

    public void ClearActions()
    {
        currentPossibleActions = null;
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
