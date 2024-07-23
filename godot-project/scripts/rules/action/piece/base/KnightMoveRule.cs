using Godot;
using Godot.Collections;
using System;

public partial class KnightMoveRule : ActionRuleBase
{
    public override void AddPossibleActions(GameController game, Piece piece)
    {
        Vector2I thisPosition = piece.cell.pos;

        Attack(piece, thisPosition + new Vector2I(-2, -1), AttackType.IfMove); // Up Left (Lower)
        Attack(piece, thisPosition + new Vector2I(-1, -2), AttackType.IfMove); // Up Left (Upper)

        Attack(piece, thisPosition + new Vector2I(2, -1), AttackType.IfMove); // Up Right (Lower)
        Attack(piece, thisPosition + new Vector2I(1, -2), AttackType.IfMove); // Up Right (Upper)

        Attack(piece, thisPosition + new Vector2I(-1, 2), AttackType.IfMove); // Down Left (Lower)
        Attack(piece, thisPosition + new Vector2I(-2, 1), AttackType.IfMove); // Down Left (Upper)

        Attack(piece, thisPosition + new Vector2I(1, 2), AttackType.IfMove); // Down Right (Lower)
        Attack(piece, thisPosition + new Vector2I(2, 1), AttackType.IfMove); // Down Right (Upper)
    }
}
