
using Godot;

public partial class BiggerBoardCard : CardBase
{
    private const int SizeIncrease = 2;
    public override void OnAddCard(GameState game)
    {
        Vector2I sizeChange = new Vector2I(SizeIncrease, SizeIncrease);
        game.gridUpperCorner += sizeChange;
        game.gridLowerCorner -= sizeChange;
    }

    public override CardBase Clone()
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