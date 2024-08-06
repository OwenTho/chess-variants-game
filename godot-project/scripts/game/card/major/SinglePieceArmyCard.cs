using Godot;
using System;
using Godot.Collections;

public partial class SinglePieceArmyCard : CardBase
{
    public string armyPiece = "";
    
    public override void MakeListeners(GameEvents gameEvents)
    {
        // gameEvents.AddListener(new EventListener(GameEvents.StartGame, OnStartGame));
    }

    public override void OnAddCard(GameState game)
    {
        // Get all pieces, and change them into the selected piece
        if (!game.TryGetPieceInfo(armyPiece, out PieceInfo pieceInfo))
        {
            GD.PushError($"SinglePieceArmy couldn't create an army of {armyPiece} as it couldn't get the PieceInfo.");
            return;
        }
        foreach (var piece in game.allPieces)
        {
            piece.info = pieceInfo;
        }

        game.KingId = armyPiece;
    }
    
    public override CardBase Clone()
    {
        SinglePieceArmyCard newCard = new SinglePieceArmyCard();
        newCard.armyPiece = armyPiece;
        return newCard;
    }

    protected override Dictionary<string, string> ToDict(GameState game)
    {
        Dictionary<string, string> cardData = new Dictionary<string, string>();
        cardData.Add("army_piece", armyPiece);
        return cardData;
    }

    public override void FromDict(GameState game, Dictionary<string, string> dataDict)
    {
        if (!dataDict.TryGetValue("army_piece", out armyPiece))
        {
            GD.PushError("Data did not contain the value for the army piece.");
        }
    }

    public override string GetName()
    {
        return "Single Piece Army";
    }

    public override string GetDescription()
    {
        return $"All pieces on the board will become a [color=aqua]{StringUtil.ToTitleCase(armyPiece)}[/color].";
    }
}
