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
                AttackAction newAttack = new AttackAction(piece, actionPos, actionPos);
                possibleActions.Add(newAttack);

                MoveAction newMove = new MoveAction(piece, actionPos, actionPos);
                newMove.tags.Add("line_move");
                possibleActions.Add(newMove);

                newMove.attackAction = newAttack;
                newAttack.moveAction = newMove;
                newAttack.AddDependency(newMove);

                if (prevMove != null)
                {
                    newMove.AddDependency(prevMove);
                }

                prevMove = newMove;
            }
        }
        return possibleActions;
    }

    internal abstract Vector2I[] GetDirs();
}
