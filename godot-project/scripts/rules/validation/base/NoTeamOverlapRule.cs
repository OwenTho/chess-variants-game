using Godot.Collections;

internal partial class NoTeamOverlapRule : ValidationRuleBase {
  public override void CheckAction(GameState game, Piece piece, ActionBase action) {
    // Only continue if action is a move action
    if (action is not MoveAction) {
      return;
    }

    var moveAction = (MoveAction)action;
    // Check if there is at least ONE team piece. If there is, add the enemy_overlap tag.
    if (game.TryGetPiecesAt(moveAction.MoveLocation.X, moveAction.MoveLocation.Y,
          out Array<Piece> pieces)) {
      if (HasTeamPieces(pieces, piece.TeamId)) {
        action.InvalidTag("team_overlap");
      }
    }
  }
}