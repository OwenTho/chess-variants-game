using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal partial class AttackNeedsTargetRule : ValidationRuleBase
{
    public override void CheckAction(GameController game, Piece piece, ActionBase action)
    {
        // Ignore if not attack
        if (action is not AttackAction)
        {
            return;
        }
        // If it has specific targets, it's valid
        AttackAction attackAction = (AttackAction)action;
        if (attackAction.HasSpecificVictims())
        {
            return;
        }
        // If it doesn't, check if there are targets it will attack
        if (attackAction.HasTargetedPieces(game))
        {
            return;
        }

        // If none of the above are true, then add the invalid tag
        attackAction.InvalidTag("no_attack_target");
    }
}
