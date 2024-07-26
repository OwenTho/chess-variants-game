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
            if (!game.HasPieceIdAt("king", moveAction.moveLocation.X, moveAction.moveLocation.Y))
            {
                moveAction.VerifyTagDependents("no_check");
            }
            if (moveAction.attackAction != null)
            {
                // Remove the tag from the attack action, as it is a dependent
                moveAction.attackAction.RemoveInvalidTag("line_stop");
                moveAction.attackAction.tags.Remove("no_check");
            }
        }
    }
}

