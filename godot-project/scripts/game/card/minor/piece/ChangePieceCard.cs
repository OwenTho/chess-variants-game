
using Godot;
using Godot.Collections;

public partial class ChangePieceCard : CardBase
{
    public string toPiece = "";
    
    public override void MakeListeners(GameEvents gameEvents)
    {
        AddNotice("piece_changed", OnPieceChanged);
    }

    public override void OnAddCard(GameState game)
    {
        if (!enabled)
        {
            return;
        }
        
        // When card is added, emit that the card needs to be selected and
        // then pause the game's execution
        SendCardNotice(game, "change_piece");
        Wait(game);
    }

    public void OnPieceChanged(GameState game)
    {
        EndWait(game);
        // Remove the card from the game as it has no more use.
        game.RemoveCard(this);
    }

    protected override CardBase CloneCard()
    {
        return new ChangePieceCard();
    }

    protected override Dictionary<string, string> ToDict(GameState game)
    {
        Dictionary<string, string> cardDict = new Dictionary<string, string>();
        cardDict.Add("to_piece", toPiece);
        return cardDict;
    }

    public override void FromDict(GameState game, Dictionary<string, string> dataDict)
    {
        if (!dataDict.TryGetValue("to_piece", out toPiece))
        {
            GD.PushError("Data did not contain the value for the piece to change in to.");
        }
    }

    public override string GetCardName()
    {
        return "Change Piece";
    }

    public override string GetCardDescription()
    {
        return $"Change a selected piece and its linked pieces into a {toPiece}.";
    }
}