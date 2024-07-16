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

        foreach (Vector2I dir in GetDirs())
        {
            Array<ActionBase> previousActions = null;
            for (int i = 1; i <= maxForward; i++)
            {
                Array<ActionBase> newActions = Attack(piece.grid, thisPosition + (dir * i), possibleActions, moveType: AttackType.AlsoMove);
                if (previousActions != null)
                {
                    foreach (ActionBase prevAction in previousActions)
                    {
                        foreach (ActionBase newAction in newActions)
                        {
                            newAction.DependsOn(prevAction);
                        }
                    }
                }
                previousActions = newActions;
            }
        }
        return possibleActions;
    }

    internal abstract Vector2I[] GetDirs();
}
