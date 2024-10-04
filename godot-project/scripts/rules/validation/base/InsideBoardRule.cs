internal partial class InsideBoardRule : ValidationRuleBase {
  public override void CheckAction(GameState game, Piece piece, ActionBase action) {
    if (action.ActionLocation.X < game.GridLowerCorner.X ||
        action.ActionLocation.X > game.GridUpperCorner.X) {
      action.InvalidTag("outside_board");
      return;
    }

    if (action.ActionLocation.Y < game.GridLowerCorner.Y ||
        action.ActionLocation.Y > game.GridUpperCorner.Y) {
      action.InvalidTag("outside_board");
    }
  }
}