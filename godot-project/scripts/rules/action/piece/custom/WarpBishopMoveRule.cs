using Godot;

public partial class WarpBishopMoveRule : LineRuleBase
{
    public override ActionBase MakeActionAt(GameState game, Piece piece, Vector2I actionLocation, ActionBase prevAction, bool isLastAction)
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