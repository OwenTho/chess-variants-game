
using Godot;

public partial class SlideAttackMoveRule : RelativeLineRuleBase
{
    public SlideAttackMoveRule(LineDirectionChooserBase chooser) : base(chooser)
    {
    }

    public SlideAttackMoveRule(RelativePieceDirection[] dirs) : base(dirs)
    {
    }

    public SlideAttackMoveRule(RelativePieceDirection dir) : base(dir)
    {
    }

    public override ActionBase MakeActionAt(GameState game, Piece piece, Vector2I actionLocation, ActionBase prevAction, bool isLastAction)
    {
        ActionBase returnAction = prevAction;
        // If it's not the last action, add a slide
        if (!isLastAction)
        {
            returnAction = new SlideAction(piece, actionLocation);
            piece.AddAction(returnAction);
        }
        
        AttackAction newAttack = Attack(piece, actionLocation, AttackType.MoveIf, prevAction);
        newAttack.moveAction.Tag("line_move");

        return returnAction;
    }
}