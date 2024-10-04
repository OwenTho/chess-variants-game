using Godot;
using Godot.Collections;

public partial class PawnArmyCard : CardBase {
  public string ArmyPiece = "pawn";

  public override void OnAddCard(GameState game) {
    // If card isn't enabled, don't do anything.
    if (!Enabled) {
      return;
    }

    // Get all pieces, and change them into the selected piece
    if (!game.TryGetPieceInfo(ArmyPiece, out PieceInfo pieceInfo)) {
      GD.PushError(
        $"PawnArmyCard couldn't create an army of {ArmyPiece} as it couldn't get the PieceInfo.");
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
  }

  public override string GetCardName() {
    return "Pawn Army";
  }

  public override string GetCardDescription() {
    return
      $"All pieces on the board will become a [color={PieceNameColour}]Pawn[/color]. The [color={PieceNameColour}]Pawn[/color] will become the King piece.";
  }

  protected override CardBase CloneCard() {
    var newCard = new PawnArmyCard();
    newCard.ArmyPiece = ArmyPiece;
    return newCard;
  }
}