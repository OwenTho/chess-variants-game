using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal abstract partial class LineMoveRule : ActionRuleBase
{

    public override Array<ActionBase> AddPossibleActions(GameController game, Piece piece, Array<ActionBase> possibleActions)
    {
        int maxForward = piece.info.level;
        Vector2I thisPosition = new Vector2I(piece.cell.x, piece.cell.y);

        foreach (Vector2I dir in GetDirs())
        {
            ActionBase prevMove = null;
            for (int i = 1; i <= maxForward; i++)
            {
                Vector2I actionPos = thisPosition + (dir * i);
                AttackAction newAttack = Attack(game.grid, piece, actionPos, possibleActions, AttackType.IfMove, prevMove);
                newAttack.moveAction.tags.Add("line_move");

                prevMove = newAttack.moveAction;
            }
        }
        return possibleActions;
    }

    internal abstract Vector2I[] GetDirs();
}
