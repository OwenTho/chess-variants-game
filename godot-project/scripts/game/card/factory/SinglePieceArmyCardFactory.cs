public partial class SinglePieceArmyCardFactory : CardFactory {
  public override bool CanMakeNewCard(GameState game) {
    return true;
  }

  protected override CardBase CreateCard(GameState game) {
    // Make a new card
    var newCard = new SinglePieceArmyCard();
    // Get all the pieces in the game
    string[] pieceIds = game.GetAllPieceIds();
    // Randomly select a piece id from the list
    int randomId = game.GameRandom.RandiRange(0, pieceIds.Length - 1);
    // Set the army piece to be the randomly selected piece
    newCard.ArmyPiece = pieceIds[randomId];
    return newCard;
  }

  protected override CardBase CreateBlankCard(GameState game) {
    return new SinglePieceArmyCard();
  }
}