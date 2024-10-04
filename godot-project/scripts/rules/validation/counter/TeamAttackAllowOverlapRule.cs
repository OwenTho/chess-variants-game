using Godot.Collections;

public partial class TeamAttackAllowOverlapRule : ValidationRuleBase {
  public override void CheckAction(GameState game, Piece piece, ActionBase action) {
    // Only continue if action is an attack action
    if (action is not AttackAction attackAction) {
      return;
    }

    // If there is no move action for the attack, just ignore
    if (attackAction.MoveAction == null) {
      return;
    }

    // If it has a specific target, remove team_overlap as it doesn't matter
    if (attackAction.HasSpecificVictims()) {
      if (game.TryGetPiece(attackAction.SpecificVictimId, out Piece specificVictim)) {
        if (specificVictim.TeamId == piece.TeamId) {
          attackAction.MoveAction.RemoveInvalidTag("team_overlap");
        }
      }

      return;
    }

    // Check if there is at least ONE team piece. If there is, remove the enemy_overlap tag.
    if (game.TryGetPiecesAt(attackAction.AttackLocation.X, attackAction.AttackLocation.Y,
          out Array<Piece> victims)) {
      if (HasTeamPieces(victims, piece.TeamId)) {
        attackAction.MoveAction.RemoveInvalidTag("team_overlap");
      }
    }
  }
}