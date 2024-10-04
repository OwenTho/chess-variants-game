public partial class BlockadeNoAttackRule : ValidationRuleBase {
  public override void CheckAction(GameState game, Piece piece, ActionBase action) {
    if (action is not AttackAction attackAction) {
      return;
    }

    attackAction.InvalidTag("blockade_no_attack", ActionBase.CarryType.None);
    attackAction.MoveAction?.InvalidTag("blockade_no_attack", ActionBase.CarryType.None);
  }
}