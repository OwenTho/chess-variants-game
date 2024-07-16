internal partial class NoTeamAttack : ValidationRuleBase
{
    public override void CheckAction(Piece piece, ActionBase action, Tags invalidTags, Tags extraTags)
    {
        // Only continue if action is an attack action
        if (action is AttackAction)
        {
            return;
        }
        if (piece.grid.TryGetCellAt(action.actionLocation.X, action.actionLocation.Y, out GridCell cell))
        {
            // If there is a cell, and the piece is a teammate, then make invalid
            GridItem item = cell.GetItem(0);
            if (item is Piece)
            {
                Piece otherPiece = (Piece)item;
                if (otherPiece.teamId == piece.teamId)
                {
                    invalidTags.Add("team_attack");
                }
            }
            // If nothing there, ignore
        }
    }
}
