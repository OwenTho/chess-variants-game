using System.Linq;
using Godot;
using Godot.Collections;

public partial class PromotionCard : CardBase {
  public const string PromotedTag = "just_promoted";

  public string FromPiece = "";
  public Piece PromotingPiece;
  public Array<string> ToPiece = new();

  public override void MakeListeners(GameEvents gameEvents) {
    AddListener(gameEvents, GameEvents.PieceMoved, OnPieceMove, GetPromotionCardEventFlags);
    AddListener(gameEvents, GameEvents.EndTurn, OnEndTurn);

    AddNotice("promoted", PiecePromoted);
  }

  public override CardReturn OnMatchingCard(CardBase card) {
    // Card is matching, so should also be a promotion card
    var newCard = (PromotionCard)card;
    // If the fromPiece matches...
    if (FromPiece == newCard.FromPiece) {
      // Combine the arrays
      ToPiece.AddRange(newCard.ToPiece);
      return CardReturn.Combined;
    }

    return CardReturn.Ignored;
  }

  public EventResult GetPromotionCardEventFlags(GameState game) {
    // If not server, just continue
    if (!game.IsServer) {
      return EventResult.Continue;
    }

    if (ToPiece != null && ToPiece.Count > 1) {
      if (game.LastMovePiece.Info != null && game.LastMovePiece.Info.PieceId == FromPiece &&
          game.IsPieceAtEndOfBound(game.LastMovePiece)) {
        return EventResult.Wait;
      }
    }

    return EventResult.Continue;
  }

  public void OnPieceMove(GameState game) {
    // Ignore if not the right piece
    if (game.LastMovePiece.Info == null || game.LastMovePiece.Info.PieceId != FromPiece) {
      game.EndEventsWait();
      return;
    }

    // If there are no options, ignore
    if (ToPiece == null || ToPiece.Count == 0) {
      game.EndEventsWait();
      return;
    }

    // Only if the piece has reached the end of the board...
    if (!game.IsPieceAtEndOfBound(game.LastMovePiece)) {
      game.EndEventsWait();
      return;
    }

    // If the piece has the 'just_promoted' tag, ignore
    if (game.LastMovePiece.HasTag(PromotedTag)) {
      game.EndEventsWait();
      return;
    }

    // If there is only one toPiece, just set it
    if (ToPiece.Count == 1) {
      if (!game.TryGetPieceInfo(ToPiece[0], out PieceInfo info)) {
        GD.PushError(
          $"Tried to promote a piece to {ToPiece[0]}, but such a PieceInfo does not exist.");
        return;
      }

      game.LastMovePiece.Info = info;
      return;
    }

    // If there is more than one option, output the signal that the piece needs to be promoted.
    // Only server should do this.
    if (!game.IsServer) {
      return;
    }

    PromotingPiece = game.LastMovePiece;
    PromotingPiece.Tags.Add(PromotedTag);
    SendCardNotice(game, "promotion");
  }

  public void OnEndTurn(GameState game) {
    // Remove the 'just_promoted' tag from all pieces
    // This limits pieces to 1 promotion per turn
    foreach (Piece piece in game.AllPieces) {
      piece.Tags.Remove(PromotedTag);
    }
  }

  public void PiecePromoted(GameState game) {
    EndWait(game);
  }


  public override void FromDict(GameState game, Dictionary<string, string> dataDict) {
    if (!dataDict.TryGetValue("from_piece", out FromPiece)) {
      GD.PushError("Data did not contain the value for the army piece.");
    }

    if (dataDict.TryGetValue("total_promos", out string promoCountString)) {
      if (int.TryParse(promoCountString, out int promoCount)) {
        for (int i = 0; i < promoCount; i++) {
          if (dataDict.TryGetValue($"promo_{i}", out string promoId)) {
            ToPiece.Add(promoId);
          }
          else {
            GD.PushError($"Tried to get promo_{i}, but it wasn't provided in the data.");
          }
        }
      }
      else {
        GD.PushError("total_promos value was not an integer.");
      }
    }
    else {
      GD.PushError("Data did not contain the number of promos for the card.");
    }
  }

  public override string GetCardName() {
    return "Promotion";
  }

  public override string GetCardDescription(GameState game) {
    return
      $"[color={PieceNameColour}]{game.GetPieceName(FromPiece)}[/color] can promote into [color={PieceNameColour}]{game.GetPieceName(ToPiece.FirstOrDefault())}[/color] when they reach the opposite side of the board.";
  }


  protected override CardBase CloneCard() {
    var newCard = new PromotionCard();
    newCard.FromPiece = FromPiece;
    newCard.ToPiece = ToPiece.Duplicate();
    return newCard;
  }

  protected override Dictionary<string, string> ToDict(GameState game) {
    var cardData = new Dictionary<string, string>();
    cardData.Add("from_piece", FromPiece);
    cardData.Add("total_promos", ToPiece.Count.ToString());
    for (int i = 0; i < ToPiece.Count; i++) {
      cardData.Add($"promo_{i}", ToPiece[i]);
    }

    return cardData;
  }
}