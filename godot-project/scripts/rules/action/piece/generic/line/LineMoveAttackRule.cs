
using Godot;

public partial class LineMoveAttackRule : RelativeLineRuleBase
{
    public LineMoveAttackRule(LineDirectionChooserBase chooser) : base(chooser)
    {
        
    }
    
    public LineMoveAttackRule(RelativePieceDirection[] dirs) : base(dirs)
    {
    }

    public LineMoveAttackRule(RelativePieceDirection dir) : base(dir)
    {
    }

    public override ActionBase MakeActionAt(GameState game, Piece piece, Vector2I actionLocation, ActionBase prevAction, bool isLastAction)
    {
        if (prevAction is not MoveAction)
        {
            prevAction = null;
        }
        AttackAction newAttack = Attack(piece, actionLocation, AttackType.IfMove, prevAction);
        newAttack.moveAction.Tag("line_move", ActionBase.CarryType.None);

        prevAction = newAttack.moveAction;
        OnNewLineAction(game, piece, newAttack.moveAction, newAttack);
        return prevAction;
    }
    
    public virtual void OnNewLineAction(GameState game, Piece piece, MoveAction moveAction, AttackAction attackAction)
    {
        
    }
}