public partial class SimpleCardFactory<T> : CardFactory where T : CardBase, new() {
  public override bool CanMakeNewCard(GameState game) {
    return true;
  }

  protected override CardBase CreateCard(GameState game) {
    return new T();
  }

  protected override CardBase CreateBlankCard(GameState game) {
    return new T();
  }
}