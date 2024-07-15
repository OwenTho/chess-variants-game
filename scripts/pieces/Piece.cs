using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class Piece : GridItem
{
	public int timesMoved = 0;
	public PieceInfo info { get; internal set; }
	public int pieceId;
	public Vector2I forwardDirection = Vector2I.Down;

	public HashSet<string> tags = new HashSet<string>();

	public Array<ActionBase> GetPossibleActions()
	{
		GD.Print($"Looking through rules: {info.rules.Count}");
        Array<ActionBase> allPossibleActions = new Array<ActionBase>();
		foreach (PieceRule pieceRule in info.rules)
        {
            GD.Print($"Rule: {pieceRule}");
            if (pieceRule.isEnabled)
			{
				pieceRule.rule.AddPossibleActions(this, allPossibleActions);
			}
		}
		return allPossibleActions;
	}
}
