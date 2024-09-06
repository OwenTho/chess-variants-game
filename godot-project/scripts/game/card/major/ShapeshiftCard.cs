
public partial class ShapeshiftCard : CardBase
{
    public override void MakeListeners(GameEvents gameEvents)
    {
        AddListener(gameEvents, GameEvents.PieceTaken, OnPieceTaken);
    }

    public void OnPieceTaken(GameState game)
    {
        if (game.lastAttackerPiece == null || game.lastTakenPiece == null)
        {
            return;
        }

        game.lastAttackerPiece.info = game.lastTakenPiece.info;
    }
    
    protected override CardBase CloneCard()
    {
        return new ShapeshiftCard();
    }

    public override string GetCardName()
    {
        return "Shapeshift";
    }

    public override string GetCardDescription()
    {
        return "Upon taking a piece, changes the [color=red]Attacking piece[/color] into the piece that was taken.";
    }
}