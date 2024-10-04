using Godot;
using Godot.Collections;

public partial class AttackAction : ActionBase {
  public Vector2I AttackLocation;

  // Only on server
  public MoveAction MoveAction;
  public int SpecificVictimId = -1; // Leave null unless needed

  public AttackAction() {
  }

  public AttackAction(Piece owner, Vector2I actionLocation, Vector2I attackLocation,
    MoveAction moveAction = null) :
    base(owner, actionLocation) {
    AttackLocation = attackLocation;
    MoveAction = moveAction;
  }

  public void SetVictim(int pieceId) {
    SpecificVictimId = pieceId;
  }

  public bool HasSpecificVictims() {
    if (SpecificVictimId <= -1) {
      return false;
    }

    return true;
  }

  public Array<Piece> GetTargetedPieces(GameState game) {
    return game.GetPiecesAt(AttackLocation.X, AttackLocation.Y);
  }

  public bool HasTargetedPieces(GameState game) {
    return game.HasPieceAt(AttackLocation.X, AttackLocation.Y);
  }


  public override void ActOn(GameState game, Piece piece) {
    // Announce the event, and stop if cancelled / not continuing
    if (!game.GameEvents.AnnounceEvent(GameEvents.AttackPiece)) {
      return;
    }

    // If there are special victims, only take those
    if (HasSpecificVictims()) {
      game.TakePiece(SpecificVictimId, piece.Id);
      return;
    }

    // Otherwise, if there are no victims, just take whatever isn't on this
    // piece's team.
    Array<Piece> targetedPieces = GetTargetedPieces(game);
    foreach (Piece victim in targetedPieces) {
      // If it's the piece acting, don't take it to avoid errors.
      // To do this, the specific victim should be set to the acting piece.
      if (piece == victim) {
        continue;
      }

      // Take the piece
      game.TakePiece(victim, piece);
    }
  }

  public override Dictionary<string, string> ToDict() {
    Dictionary<string, string> actionDict = new();
    AddVector2IToDict("attack_loc", AttackLocation, actionDict);
    if (HasSpecificVictims()) {
      actionDict.Add("victim_id", SpecificVictimId.ToString());
    }

    return actionDict;
  }

  public override void FromDict(Dictionary<string, string> actionDict) {
    AttackLocation = ReadVector2IFromDict("attack_loc", actionDict);
    if (actionDict.TryGetValue("victim_id", out string dictVictimId)) {
      if (int.TryParse(dictVictimId, out int victimId)) {
        SpecificVictimId = victimId;
      }
      else {
        GD.PushError("victim_id is not an int.");
      }
    }
    else {
      SpecificVictimId = -1;
    }
  }


  protected override ActionBase Clone() {
    var newAttack = new AttackAction(null, ActionLocation, AttackLocation);
    return newAttack;
  }
}