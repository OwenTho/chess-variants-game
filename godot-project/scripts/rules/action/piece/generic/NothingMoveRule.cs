public partial class NothingMoveRule : ActionRuleBase {
  public override void AddPossibleActions(GameState game, Piece piece, int level) {
    piece.AddAction(new NothingAction(piece, piece.Cell.Pos));
  }
}