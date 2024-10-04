using Godot;

internal partial class PawnMoveAction : MoveAction {
  public PawnMoveAction() {
  }

  public PawnMoveAction(Piece owner, Vector2I actionLocation, Vector2I moveLocation) : base(owner,
    actionLocation,
    moveLocation) {
  }

  public override void ActOn(GameState game, Piece piece) {
    base.ActOn(game, piece);
    piece.Tags.Add("pawn_initial");
  }

  protected override ActionBase Clone() {
    var newAction = new PawnMoveAction(null, ActionLocation, MoveLocation);
    return newAction;
  }
}