using Godot.Collections;

internal partial class NoTeamAttackRule : ValidationRuleBase {
  public override void CheckAction(GameState game, Piece piece, ActionBase action) {
    // Only continue if action is an attack action
    if (action is not AttackAction attackAction) {
      return;
    }

    // If specific targets, it's invalid if it's a teammate
    if (attackAction.HasSpecificVictims()) {
      if (game.TryGetPiece(attackAction.SpecificVictimId, out Piece specificVictim)) {
        if (specificVictim.TeamId == piece.TeamId) {
          attackAction.MoveAction.RemoveInvalidTag("team_overlap");
        }
      }
    }

    // If the victim and attacker are on the same side, make the attack invalid
    if (game.TryGetPiecesAt(attackAction.AttackLocation.X, attackAction.AttackLocation.Y,
          out Array<Piece> pieces)) {
      if (HasTeamPieces(pieces, piece.TeamId)) {
        action.InvalidTag("team_attack");
      }
    }
  }
}