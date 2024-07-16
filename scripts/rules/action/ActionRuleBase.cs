using Godot.Collections;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract partial class ActionRuleBase : RuleBase
{
    public abstract Array<ActionBase> AddPossibleActions(GameController game, Piece piece, Array<ActionBase> possibleActions);

    public Array<ActionBase> GetPossibleActions(GameController game, Piece piece)
    {
        return AddPossibleActions(game, piece, new Array<ActionBase>());
    }

    public enum AttackType
    {
        None, // No move
        AndMove, // Move if attack
        AlsoMove // Move
    }

    // Returns the newly created rules
    internal Array<ActionBase> Attack(Grid grid, Vector2I attackLocation, Array<ActionBase> possibleActions, AttackType moveType = AttackType.AndMove, ActionBase dependentRule = null)
    {
        Array<ActionBase> newRules = new Array<ActionBase>();
        if (grid.TryGetCellAt(attackLocation.X, attackLocation.Y, out GridCell cell))
        {
            foreach (GridItem item in cell.items)
            {
                if (item is Piece)
                {
                    AttackAction newAttack = new AttackAction(attackLocation, (Piece)item);
                    if (dependentRule != null)
                    {
                        newAttack.DependsOn(newAttack);
                    }
                    possibleActions.Add(newAttack);
                    newRules.Add(newAttack);
                    if (moveType == AttackType.AndMove)
                    {
                        MoveAction newMove = new MoveAction(attackLocation, attackLocation);
                        newMove.DependsOn(newAttack);
                        possibleActions.Add(newMove);
                        newRules.Add(newMove);
                    }
                }
            }
        }
        if (moveType == AttackType.AlsoMove)
        {
            MoveAction newMove = new MoveAction(attackLocation, attackLocation);
            if (dependentRule != null)
            {
                newMove.DependsOn(dependentRule);
            }
            possibleActions.Add(newMove);
            newRules.Add(newMove);
        }
        return newRules;
    }
    
    public abstract void NewTurn(Piece piece);
}
