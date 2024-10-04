using Godot;
using Godot.Collections;

internal partial class PawnMoveRule : ActionRuleBase {
  public override void AddPossibleActions(GameState game, Piece piece, int maxForward) {
    // Only on first turn, allow an extra space forward past the level.
    var thisPosition = new Vector2I(piece.Cell.X, piece.Cell.Y);
    if (piece.TimesMoved == 0) {
      SlideAction lastMove = null;
      for (int i = 1; i <= maxForward; i++) {
        Vector2I slidePos = thisPosition + piece.ForwardDirection.AsVector() * i;
        var newSlide = new SlideAction(piece, slidePos);
        if (lastMove != null) {
          newSlide.AddDependency(lastMove);
        }

        piece.AddAction(newSlide);
        lastMove = newSlide;
      }

      // Allow an extra space forward for the first turn
      Vector2I actionPos = thisPosition + piece.ForwardDirection.AsVector() * (maxForward + 1);
      var newMove = new PawnMoveAction(piece, actionPos, actionPos);
      if (lastMove != null) {
        newMove.AddDependency(lastMove);
      }

      // The action is unique, due to needing to allow En passant
      piece.AddAction(newMove);
    }

    // If next to a piece that moved twice, allow En passant
    CheckEnPassant(game, piece, thisPosition + GridVectors.Left);
    CheckEnPassant(game, piece, thisPosition + GridVectors.Right);
  }

  public override void EndTurn(GameState game, Piece piece) {
    // If next to a pawn, EnPassant needs another check
    if (piece.Tags.Contains("by_pawn") || CheckNextToPawn(game, piece)) {
      piece.Tags.Add("by_pawn");
    }

    if (piece.Tags.Contains("by_pawn")) {
      piece.EnableActionsUpdate();
      piece.Tags.Remove("by_pawn");
    }
  }

  public override void NewTurn(GameState game, Piece piece) {
    // If next to a pawn, add the tag
    if (CheckNextToPawn(game, piece)) {
      piece.Tags.Add("by_pawn");
    }

    // If it's the piece's turn, remove pawn_initial
    if (piece.TeamId == game.CurrentPlayerNum) {
      piece.Tags.Remove("pawn_initial");
    }
  }


  private bool CheckForPawn(GameState game, Vector2I position) {
    if (game.TryGetPiecesAt(position.X, position.Y, out Array<Piece> pieces)) {
      foreach (Piece piece in pieces) {
        if (piece.Info is { PieceId: "pawn" }) {
          return true;
        }
      }
    }

    return false;
  }

  private bool CheckNextToPawn(GameState game, Piece piece) {
    return CheckForPawn(game, piece.Cell.Pos + GridVectors.Left) ||
           CheckForPawn(game, piece.Cell.Pos + GridVectors.Right);
  }

  private void CheckEnPassant(GameState game, Piece piece, Vector2I position) {
    if (game.TryGetPiecesAt(position.X, position.Y, out Array<Piece> pieces)) {
      foreach (Piece victim in pieces)
        // Ignore pieces of the same id, and that don't have the pawn_initial tag
      {
        if (victim.TeamId != piece.TeamId && victim.Tags.Contains("pawn_initial")) {
          Vector2I attackPos = position + piece.ForwardDirection.AsVector();
          var newAttack = new AttackAction(piece, attackPos, attackPos);
          newAttack.SetVictim(victim.Id);
          piece.AddAction(newAttack);

          var newMove = new MoveAction(piece, attackPos, attackPos);
          newMove.AddDependency(newAttack);
          piece.AddAction(newMove);
        }
      }
    }
  }
}