using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal abstract partial class LineMoveRule : ActionRuleBase
{

    public override Array<ActionBase> AddPossibleActions(Piece piece, Array<ActionBase> possibleActions)
    {
        int maxForward = piece.info.level;
        Vector2I thisPosition = new Vector2I(piece.cell.x, piece.cell.y);

        for (int i = 1; i <= maxForward; i++)
        {
            foreach (Vector2I dir in GetDirs())
            {
                Attack(piece.grid, thisPosition + (dir * i), possibleActions, AttackType.AlsoMove);
            }
        }
        return possibleActions;
    }

    internal abstract Vector2I[] GetDirs();
}
