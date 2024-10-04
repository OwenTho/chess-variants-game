public partial class LevelUpCard : CardBase {
  public override void MakeListeners(GameEvents gameEvents) {
    AddListener(gameEvents, GameEvents.PieceTaken, OnPieceTaken);
  }

  public override void OnAddCard(GameState game) {
    // Reset Action levels for ALL PieceInfo to 1, so that they do their minimum.
    foreach (PieceInfo pieceInfo in game.GetAllPieceInfo())
    foreach (PieceRule rule in pieceInfo.Rules) {
      rule.Level = 1;
    }
  }

  public void OnPieceTaken(GameState game) {
    Piece attacker = game.LastAttackerPiece;
    if (attacker != null) {
      attacker.Level += 1;
    }
  }

  public override string GetCardName() {
    return "Level Up";
  }

  public override string GetCardDescription() {
    return
      $"All pieces will level up on taking another piece. [color={WarningColour}]All rules will be set to level 1.[/color]";
  }

  protected override CardBase CloneCard() {
    return new LevelUpCard();
  }
}