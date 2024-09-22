
public partial class FriendlyFireCard : CardBase
{
    public override void OnAddCard(GameState game)
    {
        // Add a base rule which counters the NoTeamAttackRule
        game.AddVerificationRule(ValidationRuleIds.Counters.AllowTeamAttack);
        // Add a base rule for all pieces that allow move on attacks for team pieces.
        game.AddVerificationRule(ValidationRuleIds.Counters.TeamAttackAllowOverlap);
    }

    protected override CardBase CloneCard()
    {
        return new FriendlyFireCard();
    }

    public override string GetCardName()
    {
        return "Friendly Fire";
    }

    public override string GetCardDescription()
    {
        return "All pieces can now take, but not check, pieces on the same team.";
    }
}