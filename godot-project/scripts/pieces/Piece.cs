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

    public Array<ActionBase> GetPossibleActions(GameController game)
    {
        // GD.Print($"Looking through rules: {info.rules.Count}");
        Array<ActionBase> allPossibleActions = new Array<ActionBase>();
        foreach (PieceRule pieceRule in info.rules)
        {
            // GD.Print($"Rule: {pieceRule}");
            if (pieceRule.isEnabled)
            {
                Array<ActionBase> possibleActions = pieceRule.rule.GetPossibleActions(game, this);

                possibleActions = ValidateActions(game, possibleActions);
                if (possibleActions.Count > 0)
                {
                    allPossibleActions.AddRange(possibleActions);
                }
            }
        }
        return allPossibleActions;
    }

    public Array<ActionBase> ValidateActions(GameController game, Array<ActionBase> actions)
    {
        Array<ActionBase> validActions = new Array<ActionBase>();
        bool foundInvalid = false;

        foreach (ActionBase action in actions)
        {
            // If it's already invalid, skip
            if (!action.valid)
            {
                continue;
            }
            foreach (ValidationRuleBase rule in info.validationRules)
            {
                // GD.Print($"Checking {rule.GetType().Name} validation.");
                rule.CheckAction(game, this, action);
            }
        }

        // Check for invalid tags afterwards
        foreach (ActionBase action in actions)
        {
            // If there are no invalid tags, then it's a valid action
            if (action.invalidTags.Count == 0)
            {
                validActions.Add(action);
            }
            else
            {
                foundInvalid = true;
                action.MakeInvalid();
            }
        }

        return validActions;
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
}
