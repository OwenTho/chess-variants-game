
using System.Collections.Generic;
using Godot;

public partial class PomotionCardFactory : CardFactory
{
    private List<string> GetPossiblePromotions(GameState game)
    {
        List<CardBase> existingCards = GetExistingCards(game);
        // First, fill the possible promotions with all piece info IDs.
        List<string> possiblePromotions = new List<string>();
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
        
        // If all pieces have a matching piece info id, remove it from the options
        var pieceInfoIds = game.GetPieceIdsOnBoard();
        if (pieceInfoIds.Count == 1)
        {
            possiblePromotions.Remove(pieceInfoIds[0]);
        }
        
        // If there are no piece ids on the board, return an empty array.
        // It needs a "fromPiece" to function, but there's no valid "fromPiece" in this instance.
        if (pieceInfoIds.Count == 0)
        {
            return new List<string>();
        }

        // Finally, return the list of promotions that haven't been taken yet.
        return possiblePromotions;
    }
    
    protected override CardBase CreateCard(GameState game)
    {
        List<string> possiblePromotions = GetPossiblePromotions(game);
        if (possiblePromotions.Count == 0)
        {
            GD.PushError("Tried to make a PromotionCard, but there were no possible promotions.");
            return null;
        }
        // Randomly pick a promotion from the list
        string promotion = possiblePromotions[game.gameRandom.RandiRange(0, possiblePromotions.Count - 1)];
        if (promotion == null)
        {
            GD.PushError("Tried to make a PromotionCard, but the promotion string obtained was 'null'.");
            return null;
        }

        PromotionCard newCard = new PromotionCard();
        
        // Now pick what piece will promote into it
        List<string> validPieceIds = game.GetPieceIdsOnBoard();
        if (validPieceIds.Count == 0)
        {
            GD.PushError("Tried to make a PromotionCard, but it found no piece ids on the board.");
            return null;
        }
        // Remove the promotion piece from the list, if it's in there
        validPieceIds.Remove(promotion);
        if (validPieceIds.Count == 0)
        {
            GD.PushError("Tried to make a PromotionCard, but it found no piece ids on the board that didn't match the promotion id.");
            return null;
        }

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

        // If there is at least one promotion option, then it can promote
        return GetPossiblePromotions(game).Count > 0;
    }
}