using Godot;
using System;
using Godot.Collections;

public partial class SinglePieceArmyCard : CardBase
{
    public string armyPiece = "";
    
    public override void MakeListeners(GameEvents gameEvents)
    {
        gameEvents.AddListener(new EventListener(GameEvents.StartGame, OnStartGame));
    }

    private void OnStartGame(GameState game)
    {
        // Get all pieces, and change them into the selected piece
        if (!game.TryGetPieceInfo(armyPiece, out PieceInfo pieceInfo))
        {
            GD.PushError($"SinglePieceArmy couldn't create an army of {armyPiece} as it couldn't get the PieceInfo.");
            return;
        }
        foreach (var piece in game.allPieces)
        {
            // TODO: Change how Checkmate works so that the KingId can be changed without check if there are 2+ pieces of KingId.
            // Ignore the King
            if (piece.info != null && piece.info.pieceId == game.KingId)
            {
                continue;
            }

            piece.info = pieceInfo;
        }
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
        return $"Once the game starts, all pieces on the board will be converted into a [color=aqua]{StringUtil.ToTitleCase(armyPiece)}[/color].";
    }
}
