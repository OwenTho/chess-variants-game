using Godot.Collections;

internal partial class NoTeamAttackRule : ValidationRuleBase
{
    public override void CheckAction(GameController game, Piece piece, ActionBase action)
    {
        // Only continue if action is an attack action
        if (action is not AttackAction)
        {
            return;
        }
        AttackAction attackAction = (AttackAction)action;
        // If specific targets, exclude teammates
        if (attackAction.HasSpecificVictims())
        {
            foreach (Piece victim in attackAction.specificVictims)
            {
                if (victim.teamId == piece.teamId)
                {
                    attackAction.RemoveVictim(victim);
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
