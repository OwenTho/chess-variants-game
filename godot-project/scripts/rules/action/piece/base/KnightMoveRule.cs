using Godot;
using Godot.Collections;
using System;

public partial class KnightMoveRule : ActionRuleBase
{
    public override Array<ActionBase> AddPossibleActions(GameController game, Piece piece, Array<ActionBase> possibleActions)
    {
        Vector2I thisPosition = piece.cell.pos;

        Attack(piece.grid, piece, thisPosition + new Vector2I(-2, -1), possibleActions, AttackType.IfMove); // Up Left (Lower)
        Attack(piece.grid, piece, thisPosition + new Vector2I(-1, -2), possibleActions, AttackType.IfMove); // Up Left (Upper)

        Attack(piece.grid, piece, thisPosition + new Vector2I(2, -1), possibleActions, AttackType.IfMove); // Up Right (Lower)
        Attack(piece.grid, piece, thisPosition + new Vector2I(1, -2), possibleActions, AttackType.IfMove); // Up Right (Upper)

        Attack(piece.grid, piece, thisPosition + new Vector2I(-1, 2), possibleActions, AttackType.IfMove); // Down Left (Lower)
        Attack(piece.grid, piece, thisPosition + new Vector2I(-2, 1), possibleActions, AttackType.IfMove); // Down Left (Upper)

        Attack(piece.grid, piece, thisPosition + new Vector2I(1, 2), possibleActions, AttackType.IfMove); // Down Right (Lower)
        Attack(piece.grid, piece, thisPosition + new Vector2I(2, 1), possibleActions, AttackType.IfMove); // Down Right (Upper)
        return possibleActions;
    }
}
