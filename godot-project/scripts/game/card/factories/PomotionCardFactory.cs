
using System.Collections.Generic;
using Godot;

public partial class PomotionCardFactory : CardFactory
{
    private List<string> GetPossiblePromotions(GameState game)
    {
        List<CardBase> existingCards = GetExistingCards(game);
        List<string> possiblePromotions = new List<string>();
        // First, fill the possible promotions with all piece info IDs.
        possiblePromotions.AddRange(game.GetAllPieceIds());
        foreach (var card in existingCards)
        {
            if (card is not PromotionCard promoCard)
            {
                GD.PushError("PromotionCardFactory got a non-PromotionCard card from GetExistingCards.");
                continue;
            }

            // Loop through the promotions, and remove from the possible promotions
            foreach (var promotion in promoCard.toPiece)
            {
                possiblePromotions.Remove(promotion);
            }
        }

        // Finally, return the list of promotions that haven't been taken yet.
        return possiblePromotions;
    }
    
    protected override CardBase CreateCard(GameState game)
    {
        List<string> possiblePromotions = GetPossiblePromotions(game);
        // Randomly pick a promotion from the list
        string promotion = possiblePromotions[game.gameRandom.RandiRange(0, possiblePromotions.Count - 1)];

        PromotionCard newCard = new PromotionCard();
        
        // Now pick what piece will promote into it
        List<string> validPieceIds = new List<string>();
        validPieceIds.AddRange(game.GetAllPieceIds());
        // Remove the promotion piece from the list
        validPieceIds.Remove(promotion);

        newCard.fromPiece = validPieceIds[game.gameRandom.RandiRange(0, validPieceIds.Count - 1)];
        newCard.toPiece.Add(promotion);

        return newCard;
    }

    protected override CardBase CreateBlankCard(GameState game)
    {
        return new PromotionCard();
    }

    public override bool CanMakeNewCard(GameState game)
    {
        // If there is one or less piece id, it's not possible to promote
        if (game.GetAllPieceIds().Length <= 1)
        {
            return false;
        }
        return GetPossiblePromotions(game).Count > 0;
    }
}