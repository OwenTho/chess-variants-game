using Godot;
using System;

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
            // TODO: Change how Checkmate works so that the KingId can be changed without immediate loss.
            // Ignore the King
            if (piece.info.pieceId == "king")
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
}
