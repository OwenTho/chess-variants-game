using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal partial class LineMoveStopRule : ValidationRuleBase
{
    public override void CheckAction(GameState game, Piece piece, ActionBase action)
    {
        // If it's not a move action, ignore
        if (action is not MoveAction)
        {
            return;
        }
        // If it's not a line move, ignore
        if (!action.tags.Contains("line_move"))
        {
            return;
        }
        MoveAction moveAction = (MoveAction)action;
        if (game.HasPieceAt(moveAction.moveLocation.X, moveAction.moveLocation.Y))
        {
            // If it does, cancel movement of dependent actions if it's a line movement
            moveAction.InvalidTagDependents("line_stop");
            bool checkCanPass = false;
            if (game.HasPieceIdAt("king", moveAction.moveLocation.X, moveAction.moveLocation.Y))
            {
                // If there is a king, check can pass if king is not on the same team
                checkCanPass = true;
                // Make sure there isn't a teammate king. This prevents checking through the king
                bool sameTeam = false;
                foreach (var pieceAtPos in game.GetPiecesAt(moveAction.moveLocation.X, moveAction.moveLocation.Y))
                {
                    // Ignore if not king
                    if (pieceAtPos.info == null || pieceAtPos.info.pieceId != "king")
                    {
                        continue;
                    }
                    // If it's a king with the same id, break from the loop
                    if (pieceAtPos.teamId == piece.teamId)
                    {
                        checkCanPass = false;
                    }
                }
            }
            if (!checkCanPass)
            {
                // If check can't pass through, add the tag to the dependent actions
                moveAction.VerifyTagDependents("no_check");
                if (moveAction.attackAction != null)
                {
                    moveAction.attackAction.RemoveVerifyTag("no_check");
                }
            }
            if (moveAction.attackAction != null)
            {
                // Remove the tag from the attack action, as it is a dependent
                moveAction.attackAction.RemoveInvalidTag("line_stop", ActionBase.CarryType.NONE);
            }
        }
    }
}

