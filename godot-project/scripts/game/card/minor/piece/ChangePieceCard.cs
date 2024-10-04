using Godot;
using Godot.Collections;

public partial class ChangePieceCard : CardBase {
  public string ToPiece = "";

  public override void MakeListeners(GameEvents gameEvents) {
    AddNotice("piece_changed", OnPieceChanged);
  }

  public override void OnAddCard(GameState game) {
    if (!Enabled) {
      return;
    }

    // When card is added, emit that the card needs to be selected and
    // then pause the game's execution
    SendCardNotice(game, "change_piece");
    Wait(game);
  }

  public void OnPieceChanged(GameState game) {
    EndWait(game);
    // Remove the card from the game as it has no more use.
    game.RemoveCard(this);
  }

  public override void FromDict(GameState game, Dictionary<string, string> dataDict) {
    if (!dataDict.TryGetValue("to_piece", out ToPiece)) {
      GD.PushError("Data did not contain the value for the piece to change in to.");
    }
  }

  public override string GetCardName() {
    return "Change Piece";
  }

  public override string GetCardDescription(GameState game) {
    return
      $"Change a selected piece on your team and its linked pieces into a [color={PieceNameColour}]{game.GetPieceName(ToPiece)}[/color].";
  }


  protected override CardBase CloneCard() {
    return new ChangePieceCard();
  }

  protected override Dictionary<string, string> ToDict(GameState game) {
    var cardDict = new Dictionary<string, string>();
    cardDict.Add("to_piece", ToPiece);
    return cardDict;
  }
}