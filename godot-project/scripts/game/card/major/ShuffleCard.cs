using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ShuffleCard : CardBase
{
    public override void MakeListeners(GameEvents gameEvents)
    {
        // TODO: Implement server -> client shuffle
        // Wait for the event to finish, as the server needs to tell all the clients what to shuffle
        gameEvents.AddListener(new EventListener(GameEvents.StartGame, OnStartGame, (game) => (EventResult.Wait)));
    }

    public void OnStartGame(GameState game)
    {
        // TODO: Disable Client shuffling
        // Shuffle the positions of all pieces by making two identical arrays of the pieceIds,
        // and then randomising one of them.
        List<int> unshuffled = new List<int>();
        List<int> shuffled = new List<int>();
        Dictionary<int, List<Piece>> linkToPiece = new Dictionary<int, List<Piece>>();
        Dictionary<int, Vector2I> piecePos = new Dictionary<int, Vector2I>();
        
        foreach (var piece in game.allPieces)
        {
            if (!linkToPiece.TryGetValue(piece.linkId, out List<Piece> pieces))
            {
                pieces = new List<Piece>();
                linkToPiece.Add(piece.linkId, pieces);
            }

            pieces.Add(piece);
            // Add the piece's location to the dictionary
            piecePos.Add(piece.id, piece.cell.pos);
            if (unshuffled.Contains(piece.linkId))
            {
                continue;
            }
            unshuffled.Add(piece.linkId);
            // Randomly insert the id (or add if there is nothing)
            if (shuffled.Count > 0)
            {
                shuffled.Insert(game.gameRandom.RandiRange(0, shuffled.Count - 1), piece.linkId);
            }
            else
            {
                shuffled.Add(piece.linkId);
            }
        }
        
        
        // Now move the pieces. They should only swap with their teammates
        foreach (var piece in game.allPieces)
        {
            int linkFromInd = unshuffled.IndexOf(piece.linkId);
            int linkTo = shuffled[linkFromInd];

            if (linkToPiece.TryGetValue(linkTo, out List<Piece> pieces))
            {
                // Get a random teammate piece
                List<Piece> teammates = new List<Piece>();
                foreach (var swapPiece in pieces)
                {
                    if (swapPiece.teamId == piece.teamId)
                    {
                        teammates.Add(swapPiece);
                    }
                }
                
                // If there is no valid teammate, skip
                if (teammates.Count == 0)
                {
                    continue;
                }
                
                // Randomly select a teammate
                Piece randomTeammate = teammates[game.gameRandom.RandiRange(0, teammates.Count - 1)];
                
                // Remove the teammate from the array so that it's the only one piece that swaps with it
                pieces.Remove(randomTeammate);
                
                if (!piecePos.TryGetValue(randomTeammate.id, out Vector2I pos))
                {
                    GD.PushError($"Tried to Shuffle piece {piece.id} to team piece {randomTeammate.id}, but the team piece isn't on the board.");
                    continue;
                }
                
                // Swap them
                // game.PutPiece(randomTeammate, piece.cell.x, piece.cell.y);
                game.PutPiece(piece, pos.X, pos.Y);
                
            }
        }
    }

    public override CardBase Clone()
    {
        return new ShuffleCard();
    }

    public override string GetName()
    {
        return "Shuffle";
    }

    public override string GetDescription()
    {
        return "Each team has their pieces shuffled once the game starts. [i]This is mirrored between teams.[/i]";
    }
}