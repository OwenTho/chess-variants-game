
using Godot;
using Godot.Collections;

public partial class BiggerBoardCard : CardBase
{
    public const string SizeKey = "size_increase";
    private int _sizeIncrease = 2;
    public override void OnAddCard(GameState game)
    {
        // If card isn't enabled, don't do anything.
        if (!enabled)
        {
            return;
        }
        
        Vector2I sizeChange = new Vector2I(_sizeIncrease, _sizeIncrease);
        game.gridUpperCorner += sizeChange;
        game.gridLowerCorner -= sizeChange;
    }

    protected override Dictionary<string, string> ToDict(GameState game)
    {
        Dictionary<string, string> cardDict = new Dictionary<string, string>();
        cardDict.Add(SizeKey, _sizeIncrease.ToString());
        return cardDict;
    }

    public override void FromDict(GameState game, Dictionary<string, string> dataDict)
    {
        if (dataDict.TryGetValue(SizeKey, out string sizeValue))
        {
            if (!int.TryParse(sizeValue, out _sizeIncrease))
            {
                GD.PushError($"{SizeKey} was not an integer.");
            }
        }
        else
        {
            GD.PushError($"{SizeKey} was not found in card dictionary.");
        }
    }

    protected override CardBase CloneCard()
    {
        return new BiggerBoardCard();
    }

    public override string GetCardName()
    {
        return "Bigger Board";
    }

    public override string GetCardDescription()
    {
        return "Makes the board bigger by 2 spaces in all directions.";
    }
}