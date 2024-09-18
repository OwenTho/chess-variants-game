using Godot;

public abstract partial class LineMoveRule : LineRuleBase
{
    public override ActionBase MakeActionAt(GameState game, Piece piece, Vector2I actionLocation, ActionBase prevAction)
    {
        if (prevAction is not MoveAction)
        {
            prevAction = null;
        }
        AttackAction newAttack = Attack(piece, actionLocation, AttackType.IfMove, prevAction);
        newAttack.moveAction.tags.Add("line_move");

        prevAction = newAttack.moveAction;
        OnNewLineAction(game, piece, newAttack.moveAction, newAttack);
        return prevAction;
    }
    
    public virtual void OnNewLineAction(GameState game, Piece piece, MoveAction moveAction, AttackAction attackAction)
    {
        
    }
}
