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
        NoMove, // No move
        MoveIf, // Move if attack
        IfMove, // Attack if move
        AlsoMove // Move and / or attack
    }

    // Returns the newly created rules
    internal Array<ActionBase> Attack(Grid grid, Piece piece, Vector2I attackLocation, Array<ActionBase> possibleActions, AttackType moveType = AttackType.NoMove, ActionBase dependentRule = null)
    {
        Array<ActionBase> newRules = new Array<ActionBase>();
        AttackAction newAttack = new AttackAction(piece, attackLocation, attackLocation);
        if (dependentRule != null)
        {
            newAttack.AddDependency(dependentRule);
        }
        possibleActions.Add(newAttack);

        // If Move:
        if (moveType != AttackType.NoMove)
        {
            MoveAction newMove = new MoveAction(piece, attackLocation, attackLocation);
            newAttack.moveAction = newMove;
            possibleActions.Add(newMove);
            if (newAttack != null)
            {
                newAttack.moveAction = newMove;
            }
            // If AndMove, add dependency for the attack
            if (moveType == AttackType.MoveIf)
            {
                newMove.AddDependency(newAttack);
            }
            if (moveType == AttackType.IfMove)
            {
                newAttack.AddDependency(newMove);
            }
            if (dependentRule != null)
            {
                newMove.AddDependency(dependentRule);
            }
            newRules.Add(newMove);
        }
        return newRules;
    }
    
    public virtual void NewTurn(GameController game, Piece piece)
    {

    }

    public virtual void EndTurn(GameController game, Piece piece)
    {

    }
}
