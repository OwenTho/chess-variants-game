using Godot;
using Godot.Collections;
using System;

public partial class KnightMoveRule : ActionRuleBase
{
    public override Array<ActionBase> AddPossibleActions(GameController game, Piece piece, Array<ActionBase> possibleActions)
    {
        Vector2I thisPosition = piece.cell.pos;

        Attack(piece.grid, thisPosition + new Vector2I(-2, -1), possibleActions, AttackType.AlsoMove); // Up Left (Lower)
        Attack(piece.grid, thisPosition + new Vector2I(-1, -2), possibleActions, AttackType.AlsoMove); // Up Left (Upper)

        Attack(piece.grid, thisPosition + new Vector2I(2, -1), possibleActions, AttackType.AlsoMove); // Up Right (Lower)
        Attack(piece.grid, thisPosition + new Vector2I(1, -2), possibleActions, AttackType.AlsoMove); // Up Right (Upper)

        Attack(piece.grid, thisPosition + new Vector2I(-1, 2), possibleActions, AttackType.AlsoMove); // Down Left (Lower)
        Attack(piece.grid, thisPosition + new Vector2I(-2, 1), possibleActions, AttackType.AlsoMove); // Down Left (Upper)

        Attack(piece.grid, thisPosition + new Vector2I(1, 2), possibleActions, AttackType.AlsoMove); // Down Right (Lower)
        Attack(piece.grid, thisPosition + new Vector2I(2, 1), possibleActions, AttackType.AlsoMove); // Down Right (Upper)
        return possibleActions;
    }

    public override void NewTurn(Piece piece)
    {
        throw new NotImplementedException();
    }
}
