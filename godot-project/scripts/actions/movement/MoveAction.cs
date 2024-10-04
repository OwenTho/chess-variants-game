using Godot;
using Godot.Collections;

public partial class MoveAction : ActionBase {
  // Only on server
  public AttackAction AttackAction;
  public Vector2I MoveLocation;

  public MoveAction() {
  }

  public MoveAction(Piece owner, Vector2I actionLocation, Vector2I moveLocation) : base(owner,
    actionLocation) {
    MoveLocation = moveLocation;
  }

  public override void ActOn(GameState game, Piece piece) {
    // Announce the event, and stop if cancelled / not continuing
    if (!game.GameEvents.AnnounceEvent(GameEvents.MovePiece)) {
      return;
    }

    game.LastMovePiece = piece;
    // Move piece
    game.MovePiece(piece, MoveLocation.X, MoveLocation.Y);
    game.GameEvents.AnnounceEvent(GameEvents.PieceMoved);
    // Now that piece has moved, it needs to be updated
    piece.EnableActionsUpdate();
    piece.TimesMoved += 1;
  }

  public override Dictionary<string, string> ToDict() {
    Dictionary<string, string> actionDict = new();
    AddVector2IToDict("move_loc", MoveLocation, actionDict);
    return actionDict;
  }

  public override void FromDict(Dictionary<string, string> actionDict) {
    MoveLocation = ReadVector2IFromDict("move_loc", actionDict);
  }

  protected override ActionBase Clone() {
    var newMove = new MoveAction(null, ActionLocation, MoveLocation);
    return newMove;
  }
}