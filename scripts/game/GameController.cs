using Godot;
using System;

public partial class GameController : Node
{
    public Grid grid { get; private set; }

    public Vector2I gridSize = new Vector2I(8, 8);

    private int lastId = 0;

    public Piece PlacePiece(string pieceId, int linkId, int teamId, int x, int y)
    {
        PieceInfo info = pieceInfoRegistry.GetValue(pieceId);
        if (info == null)
        {
            GD.PushWarning("Tried to place a piece with {pieceId}, even though it hasn't been registered!");
            return null;
        }

        
        Piece newPiece = new Piece();
        newPiece.info = info;
        newPiece.pieceId = lastId;
        lastId += 1;
        newPiece.linkId = linkId;
        newPiece.teamId = teamId;

        newPiece.forwardDirection = GetTeamDirection(teamId);

        grid.PlaceItemAt(newPiece, x, y);

        return newPiece;
    }

    public Vector2I GetTeamDirection(int teamId)
    {
        switch (teamId)
        {
            case 0:
                return Vector2I.Down;
            case 1:
                return Vector2I.Up;
        }
        return Vector2I.Zero;
    }

    public PieceInfo GetPieceInfo(string key)
    {
        return pieceInfoRegistry.GetValue(key);
    }

    public RuleBase GetRule(string key)
    {
        return actionRuleRegistry.GetValue(key);
    }

}
