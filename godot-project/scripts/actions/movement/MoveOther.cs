using Godot;
using Godot.Collections;

public partial class MoveOther : MoveAction {
  private int _movePieceId;

  public MoveOther() {
  }

  public MoveOther(Piece owner, Vector2I actionLocation, Vector2I moveLocation, int movePieceId) :
    base(owner,
      actionLocation, moveLocation) {
    MoveLocation = moveLocation;
    _movePieceId = movePieceId;
  }

  public override void ActOn(GameState game, Piece piece) {
    // Move piece
    if (game.TryGetPiece(_movePieceId, out Piece movePiece)) {
      game.MovePiece(movePiece, MoveLocation.X, MoveLocation.Y);
      // Now that piece has moved, it needs to be updated
      movePiece.EnableActionsUpdate();
      movePiece.TimesMoved += 1;
    }
  }

  public override Dictionary<string, string> ToDict() {
    // Get the base dictionary from MoveAction
    Dictionary<string, string> actionDict = base.ToDict();

    actionDict.Add("move_piece_id", _movePieceId.ToString());
    return actionDict;
  }

  public override void FromDict(Dictionary<string, string> actionDict) {
    base.FromDict(actionDict);
    if (actionDict.TryGetValue("move_piece_id", out string dictMovePieceId)) {
      if (int.TryParse(dictMovePieceId, out int newMovePieceId)) {
        _movePieceId = newMovePieceId;
      }
      else {
        GD.PushError("move_piece_id was not a number.");
      }
    }
    else {
      GD.PushError("move_piece_id not found in dictionary.");
    }
  }

  protected override ActionBase Clone() {
    var newMove = new MoveOther(null, ActionLocation, MoveLocation, -1);
    return newMove;
  }
}