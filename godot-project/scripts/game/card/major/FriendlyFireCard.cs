
public partial class FriendlyFireCard : CardBase
{
    public override void OnAddCard(GameState game)
    {
        // Add a base rule which counters the NoTeamAttackRule
        game.AddVerificationRule("allow_team_attack");
        // Add a base rule for all pieces that allow move on attacks for team pieces.
        game.AddVerificationRule("team_attack_allow_overlap");
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
        return "All pieces can now take [i]any[/i] piece on the same team.";
    }
}