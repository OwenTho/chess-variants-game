
public partial class SimpleCardFactory<T> : CardFactory where T : CardBase, new()
{
    protected override CardBase CreateCard(GameState game)
    {
        return new T();
    }

    protected override CardBase CreateBlankCard(GameState game)
    {
        return new T();
    }

    public override bool CanMakeNewCard(GameState game)
    {
        return true;
    }
}