public partial class ShapeshiftCard : CardBase {
  public override void MakeListeners(GameEvents gameEvents) {
    AddListener(gameEvents, GameEvents.PieceTaken, OnPieceTaken);
  }

  public void OnPieceTaken(GameState game) {
    if (game.LastAttackerPiece == null || game.LastTakenPiece == null) {
      return;
    }

    game.LastAttackerPiece.Info = game.LastTakenPiece.Info;
  }

  public override string GetCardName() {
    return "Shapeshift";
  }

  public override string GetCardDescription() {
    return
      $"Upon taking a piece, changes the [color={EmphasisColour}]Attacking piece[/color] into the piece that was taken.";
  }

  protected override CardBase CloneCard() {
    return new ShapeshiftCard();
  }
}