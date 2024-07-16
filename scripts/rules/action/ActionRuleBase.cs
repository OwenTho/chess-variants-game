using Godot.Collections;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract partial class ActionRuleBase : RuleBase
{
    public abstract Array<ActionBase> AddPossibleActions(Piece piece, Array<ActionBase> possibleActions);

    public Array<ActionBase> GetPossibleActions(Piece piece)
    {
        return AddPossibleActions(piece, new Array<ActionBase>());
    }

    public enum AttackType
    {
        None, // No move
        AndMove, // Move if attack
        AlsoMove // Move
    }

    internal Array<ActionBase> Attack(Grid grid, Vector2I attackLocation, Array<ActionBase> possibleActions, AttackType moveType = AttackType.AndMove)
    {
        if (grid.TryGetCellAt(attackLocation.X, attackLocation.Y, out GridCell cell))
        {
            foreach (GridItem item in cell.items)
            {
                if (item is Piece)
                {
                    possibleActions.Add(new AttackAction(attackLocation, (Piece)item));
                    if (moveType == AttackType.AndMove)
                    {
                        possibleActions.Add(new MoveAction(attackLocation, attackLocation));
                    }
                }
            }
        }
        if (moveType == AttackType.AlsoMove)
        {
            possibleActions.Add(new MoveAction(attackLocation, attackLocation));
        }
        return possibleActions;
    }
    
    public abstract void NewTurn(Piece piece);
}
