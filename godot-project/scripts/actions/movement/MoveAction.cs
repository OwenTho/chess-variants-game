using System.Collections.Generic;
using Godot;

public partial class MoveAction : ActionBase
{
    public Vector2I moveLocation;
    public AttackAction attackAction;
    public MoveAction(Piece owner, Vector2I actionLocation, Vector2I moveLocation) : base(owner, actionLocation)
    {
        this.moveLocation = moveLocation;
    }

    public override void ActOn(GameState game, Piece piece)
    {
        // Move piece
        game.grid.PlaceItemAt(piece, moveLocation.X, moveLocation.Y);
        // Now that piece has moved, it needs to be updated
        piece.EnableActionsUpdate();
        piece.timesMoved += 1;
    }

    public override object Clone()
    {
        MoveAction newMove = new MoveAction(null, actionLocation, moveLocation);
        CloneTo(newMove);
        return newMove;
    }

    public override Dictionary<string, int> GetExtraCopyLinks()
    {
        Dictionary<string, int> newDictionary = new Dictionary<string, int>();
        if (attackAction != null)
        {
            newDictionary.Add("attackAction", attackAction.actionId);
        }

        return newDictionary;
    }

    public override void SetExtraCopyLinks(GameState game, Dictionary<string, int> extraLinks, Dictionary<int, ActionBase> links)
    {
        if (extraLinks.TryGetValue("attackAction", out int attackActionId))
        {
            if (links.TryGetValue(attackActionId, out ActionBase linkedAttackAction))
            {
                attackAction = (AttackAction)linkedAttackAction;
            }
        }
    }
}