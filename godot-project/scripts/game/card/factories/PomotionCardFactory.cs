
using System.Collections.Generic;
using Godot;

public partial class PomotionCardFactory : CardFactory
{
    private List<string> GetPieceIdsOnBoard(GameState game)
    {
        List<string> existingPieceIds = new List<string>();
        
        // Get the id of the pieces currently in the game
        foreach (var piece in game.allPieces)
        {
            if (piece.info == null || existingPieceIds.Contains(piece.info.pieceId))
            {
                continue;
            }
            existingPieceIds.Add(piece.info.pieceId); 
        }

        return existingPieceIds;
    }
    
    private List<string> GetPossiblePromotions(GameState game)
    {
        List<CardBase> existingCards = GetExistingCards(game);
        List<string> possiblePromotions = GetPieceIdsOnBoard(game);
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
        
        // If there is only one possible promotion, there has to be at least one differing piece
        if (possiblePromotions.Count == 1)
        {
            var validPromotion = true;
            foreach (var piece in game.allPieces)
            {
                if (piece.info?.pieceId == possiblePromotions[0])
                {
                    validPromotion = false;
                }
            }

            if (!validPromotion)
            {
                return new List<string>();
            }
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
        List<string> validPieceIds = GetPieceIdsOnBoard(game);
        // Remove the promotion piece from the list, if it's in there
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
        // If there is at least one promotion option, then it can promote
        return GetPossiblePromotions(game).Count > 0;
    }
}