using Godot;
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
	public Vector2I forwardDirection = Vector2I.Up;

	public HashSet<string> tags = new HashSet<string>();

	public List<ActionBase> GetPossibleActions()
	{
		List<ActionBase> allPossibleActions = new List<ActionBase>();
		foreach (PieceRule pieceRule in info.rules)
		{
			if (pieceRule.isEnabled)
			{
				pieceRule.rule.AddPossibleActions(this, allPossibleActions);
			}
		}
		return allPossibleActions;
	}

	public List<Vector2I> GetPossibleAttacks()
	{
		List<Vector2I> allPossibleAttacks = new List<Vector2I>();
		foreach (PieceRule pieceRule in info.rules)
		{
			if (pieceRule.isEnabled)
			{
				allPossibleAttacks.AddRange(pieceRule.rule.GetPossibleAttacks(this));
			}
		}
		return allPossibleAttacks;
	}
}
