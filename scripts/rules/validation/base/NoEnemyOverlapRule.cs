﻿internal partial class NoEnemyOverlapRule : ValidationRuleBase
{
    public override void CheckAction(GameController game, Piece piece, ActionBase action)
    {
        // Only continue if action is a move action
        if (action is not MoveAction)
        {
            return;
        }
        MoveAction moveAction = (MoveAction)action;
        if (piece.grid.TryGetCellAt(moveAction.moveLocation.X, moveAction.moveLocation.Y, out GridCell cell))
        {
            // If there is a cell, and the piece is not a teammate, then make invalid
            GridItem item = cell.GetItem(0);
            if (item is Piece)
            {
                Piece otherPiece = (Piece)item;
                if (otherPiece.teamId != piece.teamId)
                {
                    action.InvalidTag("enemy_overlap");
                }
            }
            // If nothing there, ignore
        }
    }
}
