
public partial class LevelUpCard : CardBase
{
    public override void MakeListeners(GameEvents gameEvents)
    {
        AddListener(gameEvents, GameEvents.PieceTaken, OnPieceTaken);
    }

    public override void OnAddCard(GameState game)
    {
        // Reset Action levels for ALL PieceInfo to 1, so that they do their minimum.
        foreach (var pieceInfo in game.GetAllPieceInfo())
        {
            foreach (var rule in pieceInfo.rules)
            {
                rule.level = 1;
            }
        }
    }

    public void OnPieceTaken(GameState game)
    {
        Piece attacker = game.lastAttackerPiece;
        if (attacker != null)
        {
            attacker.level += 1;
        }
    }

    protected override CardBase CloneCard()
    {
        return new LevelUpCard();
    }

    public override string GetCardName()
    {
        return "Level Up";
    }

    public override string GetCardDescription()
    {
        return $"All pieces will level up on taking another piece. [color={WarningColour}]All current levels will lower.[/color]";
    }
}