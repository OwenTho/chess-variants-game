﻿using Godot.Collections;

internal partial class NoTeamAttackRule : ValidationRuleBase
{
    public override void CheckAction(GameState game, Piece piece, ActionBase action)
    {
        // Only continue if action is an attack action
        if (action is not AttackAction)
        {
            return;
        }
        AttackAction attackAction = (AttackAction)action;
        // If specific targets, it's invalid if it's a teammate
        if (attackAction.HasSpecificVictims())
        {
            if (game.TryGetPiece(attackAction.specificVictimId, out Piece specificVictim))
            {
                if (specificVictim.teamId == piece.teamId)
                {
                    attackAction.moveAction.RemoveInvalidTag("team_overlap");
                }
            }
        }
        // If the victim and attacker are on the same side, make the attack invalid
        if (game.TryGetPiecesAt(attackAction.attackLocation.X, attackAction.attackLocation.Y, out Array<Piece> pieces))
        {
            if (HasTeamPieces(pieces, piece.teamId, true))
            {
                action.InvalidTag("team_attack");
            }
        }
    }
}
