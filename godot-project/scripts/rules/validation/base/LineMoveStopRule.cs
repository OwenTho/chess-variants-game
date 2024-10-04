internal partial class LineMoveStopRule : ValidationRuleBase {
  public override void CheckAction(GameState game, Piece piece, ActionBase action) {
    // If it's not a move action, ignore
    if (action is not MoveAction moveAction) {
      return;
    }

    // If it's not a line move, ignore
    if (!action.Tags.Contains("line_move")) {
      return;
    }

    if (game.HasPieceAt(moveAction.MoveLocation.X, moveAction.MoveLocation.Y)) {
      // If it does, cancel movement of dependent actions if it's a line movement
      moveAction.InvalidTagDependents("line_stop");
      bool checkCanPass = false;
      if (game.HasPieceIdAt(game.KingId, moveAction.MoveLocation.X, moveAction.MoveLocation.Y)) {
        // If there is a king, check can pass if king is not on the same team
        checkCanPass = true;
        // Make sure there isn't a teammate king. This prevents checking through the king
        foreach (Piece pieceAtPos in game.GetPiecesAt(moveAction.MoveLocation.X,
                   moveAction.MoveLocation.Y)) {
          // Ignore if not king
          if (pieceAtPos.Info == null || pieceAtPos.Info.PieceId != game.KingId) {
            continue;
          }

          // If it's a king with the same id, break from the loop
          if (pieceAtPos.TeamId == piece.TeamId) {
            checkCanPass = false;
          }
        }
      }

      if (!checkCanPass) {
        // If check can't pass through, add the tag to the dependent actions
        moveAction.VerifyTagDependents("no_check");
        moveAction.AttackAction?.RemoveVerifyTag("no_check");
      }

      moveAction.AttackAction?.RemoveInvalidTag("line_stop", ActionBase.CarryType.None);
    }
  }
}