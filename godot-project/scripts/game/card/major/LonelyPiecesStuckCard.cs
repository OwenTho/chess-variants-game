
using Godot;

public partial class LonelyPiecesStuckCard : CardBase
{
    public const string RuleId = "lonely_piece_card";
    
    public override void MakeListeners(GameEvents gameEvents)
    {
        AddListener(gameEvents, GameEvents.PreNewTurn, OnPreNewTurn);
    }

    public override void OnAddCard(GameState game)
    {
        game.AddVerificationRule(RuleId);
    }

    private readonly Vector2I[] _checkDirs =
    {
        new(1,0), // Up
        new(0,-1), // Down
        new(-1,0), // Left
        new(0,1), // Right
    };
    public void OnPreNewTurn(GameState game)
    {
        // Loop through all pieces in the game
        foreach (var piece in game.allPieces)
        {
            // Remove the lonely_piece tag
            piece.tags.Remove("lonely_piece");
            
            // Now, check if the piece is next to any other pieces
            bool foundPiece = false;
            foreach (var dir in _checkDirs)
            {
                Vector2I checkPos = piece.cell.pos + dir;
                if (game.HasPieceAt(checkPos.X, checkPos.Y))
                {
                    foundPiece = true;
                    break;
                }
            }

            // If no pieces were found, add the tag
            if (!foundPiece)
            {
                piece.tags.Add("lonely_piece");
            }
        }
    }

    protected override CardBase CloneCard()
    {
        return new LonelyPiecesStuckCard();
    }

    public override string GetCardName()
    {
        return "Lonely Pieces";
    }

    public override string GetCardDescription()
    {
        return "Pieces not adjacent to other pieces are unable to take any actions.";
    }
}