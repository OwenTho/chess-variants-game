using Godot;
using Godot.Collections;

public partial class BiggerBoardCard : CardBase {
  public const string SizeKey = "size_increase";
  private int _sizeIncrease = 2;

  public override void OnAddCard(GameState game) {
    // If card isn't enabled, don't do anything.
    if (!Enabled) {
      return;
    }

    var sizeChange = new Vector2I(_sizeIncrease, _sizeIncrease);
    game.GridUpperCorner += sizeChange;
    game.GridLowerCorner -= sizeChange;
  }

  public override void FromDict(GameState game, Dictionary<string, string> dataDict) {
    if (dataDict.TryGetValue(SizeKey, out string sizeValue)) {
      if (!int.TryParse(sizeValue, out _sizeIncrease)) {
        GD.PushError($"{SizeKey} was not an integer.");
      }
    }
    else {
      GD.PushError($"{SizeKey} was not found in card dictionary.");
    }
  }

  public override string GetCardName() {
    return "Bigger Board";
  }

  public override string GetCardDescription() {
    return
      $"Makes the board bigger by [color={EmphasisColour}][b]{_sizeIncrease}[/b][/color] spaces in all directions.";
  }

  protected override CardBase CloneCard() {
    return new BiggerBoardCard();
  }

  protected override Dictionary<string, string> ToDict(GameState game) {
    var cardDict = new Dictionary<string, string>();
    cardDict.Add(SizeKey, _sizeIncrease.ToString());
    return cardDict;
  }
}