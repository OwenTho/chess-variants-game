using Godot.Collections;

internal partial class NoEnemyOverlapRule : ValidationRuleBase {
  public override void CheckAction(GameState game, Piece piece, ActionBase action) {
    // Only continue if action is a move action
    if (action is not MoveAction moveAction) {
      return;
    }

    // Check if there is at least ONE enemy. If there is, add the enemy_overlap tag.
    if (game.TryGetPiecesAt(moveAction.MoveLocation.X, moveAction.MoveLocation.Y,
          out Array<Piece> pieces)) {
      if (HasTeamPieces(pieces, piece.TeamId, false)) {
        action.InvalidTag("enemy_overlap");
      }
    }
  }
}