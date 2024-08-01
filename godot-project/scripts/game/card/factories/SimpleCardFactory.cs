
public partial class SimpleCardFactory<T> : CardFactory where T : CardBase, new()
{
    public override CardBase CreateCard(GameState game)
    {
        return new T();
    }
}