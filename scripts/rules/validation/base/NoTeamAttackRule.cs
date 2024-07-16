internal partial class NoTeamAttackRule : ValidationRuleBase
{
    public override void CheckAction(GameController game, Piece piece, ActionBase action, Tags invalidTags, Tags extraTags)
    {
        // Only continue if action is an attack action
        if (action is not AttackAction)
        {
            return;
        }
        AttackAction attackAction = (AttackAction)action;
        // If the victim and attacker are on the same side, make the attack invalid
        if (attackAction.victim != null && attackAction.victim.teamId == piece.teamId)
        {
            invalidTags.Add("team_attack");
        }
    }
}
