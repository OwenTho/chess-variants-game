using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal partial class LineMoveStopRule : ValidationRuleBase
{
    public override void CheckAction(GameController game, Piece piece, ActionBase action)
    {
        // First check if the piece contains something
        if (game.grid.TryGetCellAt(action.actionLocation.X, action.actionLocation.Y, out GridCell cell))
        {
            // If it does, cancel movement of dependent actions if it's a line movement
            if (action.tags.Contains("line_move"))
            {
                action.InvalidTagDependents("line_stop");
            }
        }
    }
}

