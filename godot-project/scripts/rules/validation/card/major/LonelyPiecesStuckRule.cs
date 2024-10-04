public partial class LonelyPiecesStuckRule : ValidationRuleBase {
  public override void CheckAction(GameState game, Piece piece, ActionBase action) {
    // If the piece has the tag, make action invalid
    if (piece.HasTag("lonely_piece")) {
      action.MakeInvalid();
    }
  }
}