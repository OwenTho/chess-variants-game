
using Godot;

public partial class LineMoveRule : RelativeLineRuleBase
{
    public LineMoveRule(LineDirectionChooserBase chooser) : base(chooser)
    {
    }

    public LineMoveRule(RelativePieceDirection[] dirs) : base(dirs)
    {
    }

    public LineMoveRule(RelativePieceDirection dir) : base(dir)
    {
    }

    public override ActionBase MakeActionAt(GameState game, Piece piece, Vector2I actionLocation, ActionBase prevAction, bool isLastAction)
    {
        MoveAction newMove = new MoveAction(piece, actionLocation, actionLocation);
        newMove.AddDependency(prevAction);
        piece.AddAction(newMove);
        newMove.Tag("line_move", ActionBase.CarryType.None);
        
        return newMove;
    }
}