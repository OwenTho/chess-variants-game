using Godot;

public partial class WarpBishopMoveRule : LineRuleBase
{
    public override void AddPossibleActions(GameState game, Piece piece, int maxForward)
    {
        base.AddPossibleActions(game, piece, maxForward);
        // In addition to above, attack in the 4 diagonals.
        Vector2I thisLocation = piece.cell.pos;
        Attack(piece, thisLocation + GridVectors.UpLeft, AttackType.MoveIf);
        Attack(piece, thisLocation + GridVectors.UpRight, AttackType.MoveIf);
        Attack(piece, thisLocation + GridVectors.DownLeft, AttackType.MoveIf);
        Attack(piece, thisLocation + GridVectors.DownRight, AttackType.MoveIf);
    }

    public override ActionBase MakeActionAt(GameState game, Piece piece, Vector2I actionLocation, ActionBase prevAction)
    {
        MoveAction newAction = new MoveAction(piece, actionLocation, actionLocation);
        piece.AddAction(newAction);
        return newAction;
    }

    public override Vector2I[] GetDirs(GameState game, Piece piece)
    {
        return new[]
        {
            GridVectors.Up,
            GridVectors.Left,
            GridVectors.Right,
            GridVectors.Down
        };
    }
}