
public partial class ShapeshiftCard : CardBase
{
    public override void MakeListeners(GameEvents gameEvents)
    {
        gameEvents.AddListener(new EventListener(GameEvents.PieceTaken, OnPieceTaken));
    }

    public void OnPieceTaken(GameState game)
    {
        if (game.lastAttackerPiece == null || game.lastTakenPiece == null)
        {
            return;
        }

        game.lastAttackerPiece.info = game.lastTakenPiece.info;
    }
    
    public override CardBase Clone()
    {
        return new ShapeshiftCard();
    }
}