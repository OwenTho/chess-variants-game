
public partial class SinglePieceArmyCardFactory : CardFactory
{
    protected override CardBase CreateCard(GameState game)
    {
        // Make a new card
        SinglePieceArmyCard newCard = new SinglePieceArmyCard();
        // Get all the pieces in the game
        string[] pieceIds = game.GetAllPieceIds();
        // Randomly select a piece id from the list
        int randomId = game.gameRandom.RandiRange(0, pieceIds.Length - 1);
        // Set the army piece to be the randomly selected piece
        newCard.armyPiece = pieceIds[randomId];
        return newCard;
    }

    protected override CardBase CreateBlankCard(GameState game)
    {
        return new SinglePieceArmyCard();
    }

    public override bool CanMakeNewCard(GameState game)
    {
        return true;
    }
}