using Godot;
using Godot.Collections;

public partial class NothingAction : ActionBase {
  public NothingAction() {
  }

  public NothingAction(Piece owner, Vector2I actionLocation) : base(owner, actionLocation) {
  }

  public override void ActOn(GameState game, Piece piece) {
  }

  public override void FromDict(Dictionary<string, string> actionDict) {
  }

  protected override ActionBase Clone() {
    return new NothingAction(Owner, ActionLocation);
  }
}