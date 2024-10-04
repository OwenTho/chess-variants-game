using System.Collections.Generic;
using Godot.Collections;

public abstract partial class ValidationRuleBase : RuleBase {
  public Array<Piece> GetTeamPieces(Array<Piece> pieces, int teamId, bool sameTeam = true) {
    var teamPieces = new Array<Piece>();
    foreach (Piece victim in pieces) {
      if ((victim.TeamId != teamId) | sameTeam) {
        teamPieces.Add(victim);
      }
    }

    return teamPieces;
  }

  public bool TryGetTeamPieces(Array<Piece> pieces, int teamId, out Array<Piece> teamPieces,
    bool sameTeam = true) {
    teamPieces = GetTeamPieces(pieces, teamId, sameTeam);
    return teamPieces.Count > 0;
  }

  public bool HasTeamPieces(IList<Piece> pieces, int teamId, bool sameTeam = true) {
    if (pieces == null || pieces.Count == 0) {
      return false;
    }

    // If there is at least ONE enemy, remove the enemy_overlap tag 
    foreach (Piece piece in pieces) {
      if ((teamId != piece.TeamId) ^ sameTeam) {
        return true;
      }
    }

    return false;
  }

  public abstract void CheckAction(GameState game, Piece piece, ActionBase action);
}