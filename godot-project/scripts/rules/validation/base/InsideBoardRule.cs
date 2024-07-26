using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal partial class InsideBoardRule : ValidationRuleBase
{
    public override void CheckAction(GameState game, Piece piece, ActionBase action)
    {
        if (action.actionLocation.X < 0 || action.actionLocation.X >= game.gridSize.X)
        {
            action.InvalidTag("outside_board");
            return;
        }
        if (action.actionLocation.Y < 0 || action.actionLocation.Y >= game.gridSize.Y)
        {
            action.InvalidTag("outside_board");
            return;
        }
    }
}
