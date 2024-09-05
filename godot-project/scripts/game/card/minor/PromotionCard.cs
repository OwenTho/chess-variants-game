
using Godot;
using Godot.Collections;

public partial class PromotionCard : CardBase
{
    public string fromPiece = "";
    public const string promotedTag = "just_promoted";
    public Array<string> toPiece;
    public Piece promotingPiece;
    public override void MakeListeners(GameEvents gameEvents)
    {
        AddListener(gameEvents, GameEvents.PieceMoved, OnPieceMove, GetPromotionCardEventFlags);
        AddListener(gameEvents, GameEvents.EndTurn, OnEndTurn);
        
        AddNotice("promoted", PiecePromoted);
    }

    public override CardReturn OnMatchingCard(CardBase card)
    {
        // Card is matching, so should also be a promotion card
        PromotionCard newCard = (PromotionCard)card;
        // If the fromPiece matches...
        if (fromPiece == newCard.fromPiece)
        {
            // Combine the arrays
            toPiece.AddRange(newCard.toPiece);
            return CardReturn.Combined;
        }

        return CardReturn.Ignored;
    }

    public EventResult GetPromotionCardEventFlags(GameState game)
    {
        // If not server, just continue
        if (!game.isServer)
        {
            return EventResult.Continue;
        }
        if (toPiece != null && toPiece.Count > 1)
        {
            if (game.lastMovePiece.info != null && game.lastMovePiece.info.pieceId == fromPiece && game.IsPieceAtEndOfBound(game.lastMovePiece))
            {
                return EventResult.Wait;
            }
        }

        return EventResult.Continue;
    }

    public void OnPieceMove(GameState game)
    {
        // Ignore if not the right piece
        if (game.lastMovePiece.info == null || game.lastMovePiece.info.pieceId != fromPiece)
        {
            game.EndEventsWait();
            return;
        }
        
        // If there are no options, ignore
        if (toPiece == null || toPiece.Count == 0)
        {
            game.EndEventsWait();
            return;
        }
        
        // Only if the piece has reached the end of the board...
        if (!game.IsPieceAtEndOfBound(game.lastMovePiece))
        {
            game.EndEventsWait();
            return;
        }
        
        // If the piece has the 'just_promoted' tag, ignore
        if (game.lastMovePiece.HasTag(promotedTag))
        {
            game.EndEventsWait();
            return;
        }
        
        // If there is only one toPiece, just set it
        if (toPiece.Count == 1)
        {
            if (!game.TryGetPieceInfo(toPiece[0], out PieceInfo info))
            {
                GD.PushError($"Tried to promote a piece to {toPiece[0]}, but such a PieceInfo does not exist.");
                return;
            }

            game.lastMovePiece.info = info;
            return;
        }
        
        // If there is more than one option, output the signal that the piece needs to be promoted.
        // Only server should do this.
        if (!game.isServer)
        {
            return;
        }

        promotingPiece = game.lastMovePiece;
        SendCardNotice(game, "promotion");
    }

    public void OnEndTurn(GameState game)
    {
        // Remove the 'just_promoted' tag from all pieces
        // This limits pieces to 1 promotion per turn
        foreach (var piece in game.allPieces)
        {
            piece.tags.Remove(promotedTag);
        }
    }

    public void PiecePromoted(GameState game)
    {
        game.EndEventsWait();
    }

    public override CardBase Clone()
    {
        PromotionCard newCard = new PromotionCard();

        return newCard;
    }
    
    protected override Dictionary<string, string> ToDict(GameState game)
    {
        Dictionary<string, string> cardData = new Dictionary<string, string>();
        cardData.Add("from_piece", fromPiece);
        cardData.Add("total_promos", toPiece.Count.ToString());
        for (int i = 0; i < toPiece.Count; i++)
        {
            cardData.Add($"promo_{i}", toPiece[i]);
        }
        return cardData;
    }

    public override void FromDict(GameState game, Dictionary<string, string> dataDict)
    {
        if (!dataDict.TryGetValue("from_piece", out fromPiece))
        {
            GD.PushError("Data did not contain the value for the army piece.");
        }

        if (dataDict.TryGetValue("total_promos", out string promoCountString))
        {
            if (int.TryParse(promoCountString, out int promoCount))
            {
                for (int i = 0; i < promoCount; i++)
                {
                    if (dataDict.TryGetValue($"promo_{i}", out string promoId))
                    {
                        toPiece.Add(promoId);
                    }
                    else
                    {
                        GD.PushError($"Tried to get promo_{i}, but it wasn't provided in the data.");
                    }
                }
            }
            else
            {
                GD.PushError("total_promos value was not an integer.");
            }
        }
        else
        {
            GD.PushError("Data did not contain the number of promos for the card.");
        }
    }

    public override string GetCardName()
    {
        return "Promotion";
    }

    public override string GetCardDescription()
    {
        return $"{fromPiece} can promote into {toPiece} when they reach the opposite side of the board.";
    }
}