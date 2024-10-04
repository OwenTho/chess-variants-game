using Godot;

public abstract partial class LineRuleBase : ActionRuleBase {
  public abstract Vector2I[] GetDirs(GameState game, Piece piece);

  public abstract ActionBase MakeActionAt(GameState game, Piece piece, Vector2I actionLocation,
    ActionBase prevAction,
    bool isLastAction);

  public override void AddPossibleActions(GameState game, Piece piece, int maxForward) {
    var thisPosition = new Vector2I(piece.Cell.X, piece.Cell.Y);

    foreach (Vector2I dir in GetDirs(game, piece)) {
      ActionBase prevAction = null;
      for (int i = 1; i <= maxForward; i++) {
        Vector2I actionPos = thisPosition + dir * i;
        prevAction = MakeActionAt(game, piece, actionPos, prevAction, i == maxForward);
      }
    }
  }
}