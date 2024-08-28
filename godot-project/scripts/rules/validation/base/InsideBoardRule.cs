using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal partial class InsideBoardRule : ValidationRuleBase
{
    public override void CheckAction(GameState game, Piece piece, ActionBase action)
    {
        if (action.actionLocation.X < game.gridLowerCorner.X || action.actionLocation.X > game.gridUpperCorner.X)
        {
            action.InvalidTag("outside_board");
            return;
        }
        if (action.actionLocation.Y < game.gridLowerCorner.Y || action.actionLocation.Y > game.gridUpperCorner.Y)
        {
            action.InvalidTag("outside_board");
            return;
        }
    }
}
