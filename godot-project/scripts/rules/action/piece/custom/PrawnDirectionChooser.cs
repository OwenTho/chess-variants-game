public class PrawnDirectionChooser : LineDirectionChooserBase {
  public override RelativePieceDirection[] GetDirs(GameState game, Piece piece) {
    if (piece.TimesMoved % 2 == 1) {
      return new[] {
        RelativePieceDirection.ForwardRight,
        RelativePieceDirection.BackwardRight,
        RelativePieceDirection.BackwardLeft,
        RelativePieceDirection.ForwardLeft
      };
    }

    return new[] {
      RelativePieceDirection.Forward,
      RelativePieceDirection.Right,
      RelativePieceDirection.Backward,
      RelativePieceDirection.Left
    };
  }
}