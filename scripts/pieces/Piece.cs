using Godot;
using Godot.Collections;
using System.Collections.Generic;

public partial class Piece : GridItem
{
	public int timesMoved = 0;
	internal int teamId = 0;
	public PieceInfo info { get; internal set; }
	public int pieceId;
	public Vector2I forwardDirection = Vector2I.Down;

	public Tags tags = new Tags();

	public Array<ActionBase> GetPossibleActions(GameController game)
	{
		GD.Print($"Looking through rules: {info.rules.Count}");
        Array<ActionBase> allPossibleActions = new Array<ActionBase>();
		foreach (PieceRule pieceRule in info.rules)
        {
            GD.Print($"Rule: {pieceRule}");
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
		GD.Print(allPossibleActions);
		return allPossibleActions;
	}

	public Array<ActionBase> ValidateActions(GameController game, Array<ActionBase> actions)
	{
		Array<ActionBase> validActions = new Array<ActionBase>();
        bool foundInvalid = false;

        foreach (ActionBase action in actions)
		{
			Tags invalidTags = new Tags();
			Tags extraTags = new Tags();
			foreach (ValidationRuleBase rule in info.validationRules)
			{
				rule.CheckAction(game, this, action, invalidTags, extraTags);
			}
			// If there are no invalid tags, then it's a valid action
			if (invalidTags.Count == 0)
			{
				validActions.Add(action);
				action.valid = true;
			} else
			{
				foundInvalid = true;
				action.valid = false;
            }
		}

		// Now, after going through all the actions, go through again and
		// remove actions that rely on other actions
		// Has to repeat until no invalid actions are found
		while (foundInvalid)
		{
			foundInvalid = false;
			for (int i = validActions.Count - 1; i >= 0; i--)
            {
                ActionBase action = validActions[i];
                GD.Print($"Checking {action.GetClass().GetBaseName()}");
				foreach (ActionBase dependentAction in action.dependsOn)
				{
					if (!dependentAction.valid)
					{
						foundInvalid = true;
						action.valid = false;
						validActions.RemoveAt(i);
						break;
					}
				}
			}
		}

		return validActions;
	}
}
