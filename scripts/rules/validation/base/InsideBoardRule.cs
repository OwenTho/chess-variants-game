using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal partial class InsideBoardRule : ValidationRuleBase
{
    public override void CheckAction(GameController game, Piece piece, ActionBase action, Tags invalidTags, Tags extraTags)
    {
        if (action.actionLocation.X < 0 || action.actionLocation.X >= game.gridSize.X)
        {
            invalidTags.Add("outside_board");
        }
        if (action.actionLocation.Y < 0 || action.actionLocation.Y >= game.gridSize.Y)
        {
            invalidTags.Add("outside_board");
        }
    }
}
