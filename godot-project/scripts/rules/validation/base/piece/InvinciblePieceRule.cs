using Godot.Collections;

public partial class InvinciblePieceRule : ValidationRuleBase {
  public const string InvincibleTag = "invincible";
  public const string InvalidTag = "invincible_piece";

  public bool HasInvincibleTag(Piece piece) {
    return piece.Info != null && piece.Info.Tags.Contains(InvincibleTag);
  }

  public override void CheckAction(GameState game, Piece piece, ActionBase action) {
    if (action is not AttackAction attackAction) {
      return;
    }

    bool attackingInvincible = false;
    if (attackAction.HasSpecificVictims()) {
      if (game.TryGetPiece(attackAction.SpecificVictimId, out Piece victim)) {
        attackingInvincible = HasInvincibleTag(victim);
      }
    }
    else {
      Array<Piece> targetedPieces = attackAction.GetTargetedPieces(game);
      foreach (Piece targetedPiece in targetedPieces) {
        if (HasInvincibleTag(targetedPiece)) {
          attackingInvincible = true;
          break;
        }
      }
    }

    if (attackingInvincible) {
      attackAction.InvalidTag(InvalidTag, ActionBase.CarryType.None);
      attackAction.MoveAction?.InvalidTag(InvalidTag, ActionBase.CarryType.None);
    }
  }
}