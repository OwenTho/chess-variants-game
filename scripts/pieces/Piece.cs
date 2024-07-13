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

    public List<Vector2I> GetPossibleMoves()
    {
        List<Vector2I> allPossibleMoves = new List<Vector2I>();
        foreach (PieceRule pieceRule in info.rules)
        {
            if (pieceRule.isEnabled)
            {
                allPossibleMoves.AddRange(pieceRule.rule.GetPossibleMoves(this));
            }
        }
        return allPossibleMoves;
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