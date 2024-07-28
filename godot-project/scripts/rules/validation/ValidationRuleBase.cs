using Godot.Collections;
using System.Collections;
using System.Collections.Generic;

public abstract partial class ValidationRuleBase : RuleBase
{
    public abstract void CheckAction(GameState game, Piece piece, ActionBase action);

    public Array<Piece> GetTeamPieces(Array<Piece> pieces, int teamId, bool sameTeam = true)
    {
        Array<Piece> teamPieces = new Array<Piece>();
        foreach (Piece victim in pieces)
        {
            if (victim.teamId != teamId | sameTeam)
            {
                teamPieces.Add(victim);
            }
        }
        return teamPieces;
    }

    public bool TryGetTeamPieces(Array<Piece> pieces, int teamId, out Array<Piece> teamPieces, bool sameTeam = true)
    {
        teamPieces = GetTeamPieces(pieces, teamId, sameTeam);
        return teamPieces.Count > 0;
    }

    public bool HasTeamPieces(IList<Piece> pieces, int teamId, bool sameTeam = true)
    {
        if (pieces == null || pieces.Count == 0)
        {
            return false;
        }
        // If there is at least ONE enemy, remove the enemy_overlap tag 
        foreach (Piece piece in pieces)
        {
            if ((teamId != piece.teamId) ^ sameTeam)
            {
                return true;
            }
        }
        return false;
    }
}
