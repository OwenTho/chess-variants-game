
using Godot.Collections;

public partial class TeamAttackAllowOverlapRule : ValidationRuleBase
{
    public override void CheckAction(GameState game, Piece piece, ActionBase action)
    {
        
        // Only continue if action is an attack action
        if (action is not AttackAction attackAction)
        {
            return;
        }
        // If there is no move action for the attack, just ignore
        if (attackAction.moveAction == null)
        {
            return;
        }
        // If it has a specific target, remove team_overlap as it doesn't matter
        if (attackAction.HasSpecificVictims())
        {
            if (game.TryGetPiece(attackAction.specificVictimId, out Piece specificVictim))
            {
                if (specificVictim.teamId == piece.teamId)
                {
                    attackAction.moveAction.RemoveInvalidTag("team_overlap");
                }
            }
            return;
        }

        // Check if there is at least ONE team piece. If there is, remove the enemy_overlap tag.
        if (game.TryGetPiecesAt(attackAction.attackLocation.X, attackAction.attackLocation.Y, out Array<Piece> victims))
        {
            if (HasTeamPieces(victims, piece.teamId))
            {
                attackAction.moveAction.RemoveInvalidTag("team_overlap");
            }
        }
    }
}