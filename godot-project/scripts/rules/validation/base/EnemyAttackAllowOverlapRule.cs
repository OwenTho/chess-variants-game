using Godot;
using System.Linq;

internal partial class EnemyAttackAllowOverlapRule : ValidationRuleBase
{
    public override void CheckAction(GameController game, Piece piece, ActionBase action)
    {
        // Only continue if action is an attack action
        if (action is not AttackAction)
        {
            return;
        }
        AttackAction attackAction = (AttackAction)action;
        // If there is no move action for the attack, just ignore
        if (attackAction.moveAction == null)
        {
            return;
        }
        // If the victim and attacker are on different sides, remove the enemy_overlap tag
        // In this instance, movement should depend on this so just remove the tag from
        // dependents.
        if (attackAction.victim != null && attackAction.victim.teamId != piece.teamId)
        {
            attackAction.moveAction.RemoveInvalidTag("enemy_overlap");
        }
    }
}
