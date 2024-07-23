using Godot.Collections;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract partial class ActionRuleBase : RuleBase
{
    public abstract void AddPossibleActions(GameController game, Piece piece);

    public enum AttackType
    {
        NoMove, // No move
        MoveIf, // Move if attack
        IfMove, // Attack if move
        AlsoMove // Move and / or attack
    }

    // Returns the newly created rules
    internal AttackAction Attack(Piece attacker, Vector2I attackLocation, AttackType attackType = AttackType.NoMove, ActionBase dependentRule = null)
    {
        AttackAction newAttack = new AttackAction(attacker, attackLocation, attackLocation);
        if (dependentRule != null && attackType != AttackType.IfMove)
        {
            newAttack.AddDependency(dependentRule);
        }
        attacker.AddAction(newAttack);

        // If Move:
        if (attackType != AttackType.NoMove)
        {
            MoveAction newMove = new MoveAction(attacker, attackLocation, attackLocation);
            newAttack.moveAction = newMove;
            newMove.attackAction = newAttack;
            attacker.AddAction(newMove);
            // If AndMove, add dependency for the attack
            if (attackType == AttackType.MoveIf)
            {
                newMove.AddDependency(newAttack);
            }
            if (attackType == AttackType.IfMove)
            {
                newAttack.AddDependency(newMove);
            }
            if (dependentRule != null && attackType != AttackType.MoveIf)
            {
                newMove.AddDependency(dependentRule);
            }
        }
        return newAttack;
    }
    
    public virtual void NewTurn(GameController game, Piece piece)
    {

    }

    public virtual void EndTurn(GameController game, Piece piece)
    {

    }
}
