using Godot;
using System;
using Godot.Collections;

public partial class PawnArmyCard : CardBase
{
    public string armyPiece = "pawn";

    public override void OnAddCard(GameState game)
    {
        // If card isn't enabled, don't do anything.
        if (!enabled)
        {
            return;
        }
        
        // Get all pieces, and change them into the selected piece
        if (!game.TryGetPieceInfo(armyPiece, out PieceInfo pieceInfo))
        {
            GD.PushError($"PawnArmyCard couldn't create an army of {armyPiece} as it couldn't get the PieceInfo.");
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
        PawnArmyCard newCard = new PawnArmyCard();
        newCard.armyPiece = armyPiece;
        return newCard;
    }

    public override void FromDict(GameState game, Dictionary<string, string> dataDict)
    {
        
    }

    public override string GetCardName()
    {
        return "Pawn Army";
    }

    public override string GetCardDescription()
    {
        return $"All pieces on the board will become a [color={PieceNameColour}]Pawn[/color]. The [color={PieceNameColour}]Pawn[/color] will become the King piece.";
    }
}
