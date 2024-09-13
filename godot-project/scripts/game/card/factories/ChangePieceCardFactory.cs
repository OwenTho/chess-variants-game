
using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class ChangePieceCardFactory : CardFactory
{
    private string FindInvalidId(GameState game)
    {
        // The only invalid id is if there is a single type of piece that differs from it
        Array<Piece> allPieces = game.allPieces;
        if (allPieces.Count == 0)
        {
            return null;
        }
        string invalidId = allPieces[0].GetPieceInfoId();
        for (var i = 1; i < allPieces.Count; i++)
        {
            if (allPieces[i].GetPieceInfoId() != invalidId)
            {
                return null;
            }
        }

        return invalidId;
    }
    
    protected override CardBase CreateCard(GameState game)
    {
        string invalidId = FindInvalidId(game);
        string pickedId;
        string[] pieceIds = game.GetAllPieceIds();
        // If it's null, just pick a random piece id
        if (invalidId == null)
        {
            pickedId = pieceIds[game.gameRandom.RandiRange(0, pieceIds.Length - 1)];
        }
        // Otherwise, if it's not null then pick any id that isn't the invalid id
        else
        {
            int invalidInd = 0;
            for (var i = 0; i < pieceIds.Length; i++)
            {
                if (pieceIds[i] == invalidId)
                {
                    invalidInd = i;
                    break;
                }
            }

            int randomInd = game.gameRandom.RandiRange(0, pieceIds.Length - 2);
            if (randomInd >= invalidInd)
            {
                randomInd += 1;
            }

            pickedId = pieceIds[randomInd];
        }

        ChangePieceCard newCard = new ChangePieceCard();
        newCard.toPiece = pickedId;

        return newCard;
    }

    protected override CardBase CreateBlankCard(GameState game)
    {
        return new ChangePieceCard();
    }

    public override bool CanMakeNewCard(GameState game)
    {
        // If there are no pieces, the card won't do anything
        int numOfPieces = game.allPieces.Count;
        if (numOfPieces == 0)
        {
            return false;
        }
        
        // Secondly, if there are only kings, and only one king on all teams,
        // then the Change Piece will always fail
        string invalidId = FindInvalidId(game);
        List<int> teams = new List<int>();
        List<int> validTeams = new List<int>();
        Array<Piece> kings = game.GetKingPieces();
        foreach (var piece in game.allPieces)
        {
            // If team isn't added, ignore
            if (!teams.Contains(piece.teamId))
            {
                teams.Add(piece.teamId);
            }
            
            // If team already validated, skip
            if (validTeams.Contains(piece.teamId))
            {
                continue;
            }
            
            // If this piece is not a king...
            if (piece.GetPieceInfoId() == game.KingId)
            {
                continue;
            }
            
            foreach (var king in kings)
            {
                // Only check other pieces
                if (piece == king)
                {
                    continue;
                }

                // Only if the link id is different
                if (piece.linkId == king.linkId)
                {
                    continue;
                }

                // If criteria above is matched, the team can have one of its kings
                // swapped to something else
                validTeams.Add(king.teamId);
            }
        }
        
        // If there are not an equal number of teams to valid teams, then
        // it can't change pieces
        if (teams.Count != validTeams.Count)
        {
            return false;
        }


        // If there is more than 1 piece info, it's possible to change the piece's info.
        // If there is only 1 piece info, there's a chance to change it from null.
        int numOfPieceInfo = game.GetAllPieceIds().Length;
        return numOfPieceInfo > 1 || (numOfPieceInfo == 1 && invalidId == null);
    }
}