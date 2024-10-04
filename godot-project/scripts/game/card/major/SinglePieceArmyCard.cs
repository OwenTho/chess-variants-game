using Godot;
using Godot.Collections;

public partial class SinglePieceArmyCard : CardBase {
  public string ArmyPiece = "";

  public override void OnAddCard(GameState game) {
    // If card isn't enabled, don't do anything.
    if (!Enabled) {
      return;
    }

    // Get all pieces, and change them into the selected piece
    if (!game.TryGetPieceInfo(ArmyPiece, out PieceInfo pieceInfo)) {
      GD.PushError(
        $"SinglePieceArmy couldn't create an army of {ArmyPiece} as it couldn't get the PieceInfo.");
      return;
    }

    foreach (Piece piece in game.AllPieces) {
      // Update the piece's info, and then mark it as needing an update.
      piece.Info = pieceInfo;
      piece.EnableActionsUpdate();
    }

    game.KingId = ArmyPiece;
  }

  public override void FromDict(GameState game, Dictionary<string, string> dataDict) {
    if (dataDict.TryGetValue("army_piece", out ArmyPiece)) {
      if (game.TryGetPieceInfo(ArmyPiece, out PieceInfo info)) {
        ArmyPiece = info.DisplayName;
      }
    }
    else {
      GD.PushError("Data did not contain the value for the army piece.");
    }
  }

  public override string GetCardName() {
    return "Single Piece Army";
  }

  public override string GetCardDescription(GameState game) {
    return
      $"All pieces on the board will become a [color=aqua]{game.GetPieceName(ArmyPiece)}[/color].";
  }


  protected override CardBase CloneCard() {
    var newCard = new SinglePieceArmyCard();
    newCard.ArmyPiece = ArmyPiece;
    return newCard;
  }

  protected override Dictionary<string, string> ToDict(GameState game) {
    var cardData = new Dictionary<string, string>();
    cardData.Add("army_piece", ArmyPiece);
    return cardData;
  }
}