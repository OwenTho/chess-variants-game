using System.Collections.Generic;
using Godot;

public partial class ShuffleCard : CardBase {
  // TODO: Have a variable shared for the shuffle "seed", rather than relying on the current seed.
  public override void OnAddCard(GameState game) {
    // If card isn't enabled, don't do anything.
    if (!Enabled) {
      return;
    }

    // TODO: While this works with matching pieces on both sides, this breaks when this is not the case.
    // Shuffle the positions of all pieces by making two identical arrays of the pieceIds,
    // and then randomising one of them.
    var unshuffled = new List<int>();
    var shuffled = new List<int>();
    var linkToPiece = new Dictionary<int, List<Piece>>();
    var piecePos = new Dictionary<int, Vector2I>();

    foreach (Piece piece in game.AllPieces) {
      if (!linkToPiece.TryGetValue(piece.LinkId, out List<Piece> pieces)) {
        pieces = new List<Piece>();
        linkToPiece.Add(piece.LinkId, pieces);
      }

      pieces.Add(piece);
      // Add the piece's location to the dictionary
      piecePos.Add(piece.Id, piece.Cell.Pos);
      if (unshuffled.Contains(piece.LinkId)) {
        continue;
      }

      unshuffled.Add(piece.LinkId);
      // Randomly insert the id (or add if there is nothing)
      if (shuffled.Count > 0) {
        shuffled.Insert(game.GameRandom.RandiRange(0, shuffled.Count - 1), piece.LinkId);
      }
      else {
        shuffled.Add(piece.LinkId);
      }
    }


    // Now move the pieces. They should only swap with their teammates
    foreach (Piece piece in game.AllPieces) {
      int linkFromInd = unshuffled.IndexOf(piece.LinkId);
      int linkTo = shuffled[linkFromInd];

      if (linkToPiece.TryGetValue(linkTo, out List<Piece> pieces)) {
        // Get a random teammate piece
        var teammates = new List<Piece>();
        foreach (Piece swapPiece in pieces) {
          if (swapPiece.TeamId == piece.TeamId) {
            teammates.Add(swapPiece);
          }
        }

        // If there is no valid teammate, skip
        if (teammates.Count == 0) {
          continue;
        }

        // Randomly select a teammate
        Piece randomTeammate = teammates[game.GameRandom.RandiRange(0, teammates.Count - 1)];

        // Remove the teammate from the array so that it's the only one piece that swaps with it
        pieces.Remove(randomTeammate);

        if (!piecePos.TryGetValue(randomTeammate.Id, out Vector2I pos)) {
          GD.PushError(
            $"Tried to Shuffle piece {piece.Id} to team piece {randomTeammate.Id}, but the team piece isn't on the board.");
          continue;
        }

        // Swap them
        // game.PutPiece(randomTeammate, piece.cell.x, piece.cell.y);
        game.PutPiece(piece, pos.X, pos.Y);
      }
    }
  }

  public override string GetCardName() {
    return "Shuffle";
  }

  public override string GetCardDescription() {
    return "Each team has their pieces shuffled. [i]This is mirrored between teams.[/i]";
  }

  protected override CardBase CloneCard() {
    return new ShuffleCard();
  }
}