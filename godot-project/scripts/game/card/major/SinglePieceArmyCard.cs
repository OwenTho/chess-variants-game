using Godot;
using System;
using Godot.Collections;

public partial class SinglePieceArmyCard : CardBase
{
    public string armyPiece = "";
    public string pieceName = "";

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
            // Update the piece's info, and then mark it as needing an update.
            piece.info = pieceInfo;
            piece.EnableActionsUpdate();
        }

        game.KingId = armyPiece;
    }
    
    protected override CardBase CloneCard()
    {
        SinglePieceArmyCard newCard = new SinglePieceArmyCard();
        newCard.armyPiece = armyPiece;
        newCard.pieceName = pieceName;
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
        if (dataDict.TryGetValue("army_piece", out armyPiece))
        {
            if (game.TryGetPieceInfo(armyPiece, out PieceInfo info))
            {
                pieceName = info.displayName;
            }
        }
        else
        {
            GD.PushError("Data did not contain the value for the army piece.");
        }
    }

    public override string GetCardName()
    {
        return "Single Piece Army";
    }

    public override string GetCardDescription()
    {
        return $"All pieces on the board will become a [color=aqua]{pieceName}[/color].";
    }
}
