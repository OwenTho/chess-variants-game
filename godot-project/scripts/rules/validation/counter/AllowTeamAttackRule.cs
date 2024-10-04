public partial class AllowTeamAttackRule : ValidationRuleBase {
  public override void CheckAction(GameState game, Piece piece, ActionBase action) {
    // Remove the "team_attack" invalid tag
    action.RemoveInvalidTag("team_attack");
  }
}