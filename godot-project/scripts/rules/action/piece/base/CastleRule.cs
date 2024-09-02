
using ChessVariantsGame.scripts.actions.movement;
using Godot;
using Godot.Collections;

public partial class CastleRule : ActionRuleBase
{
    private const int MinDist = 3;
    private const int MaxDist = 6;
    private const int ActionDist = 2;
    
    public override void AddPossibleActions(GameState game, Piece piece, int level)
    {
        // If the piece has moved, ignore
        if (piece.timesMoved > 0)
        {
            return;
        }
        // Check for castle, left and right
        CheckCastle(game, piece, GridVectors.Left);
        CheckCastle(game, piece, GridVectors.Right);
    }

    private void CheckCastle(GameState game, Piece piece, Vector2I direction)
    {
        // TODO: Use level instead of repeating 4 times
        // Repeat a number of spaces away, from a minimum distance away.
        Vector2I actionLocation = piece.cell.pos + direction * ActionDist;
        for (int i = MinDist; i <= MaxDist; i++)
        {
            Vector2I checkLocation = piece.cell.pos + direction * i;
            
            // Check the location for a piece
            if (game.TryGetPiecesAt(checkLocation.X, checkLocation.Y, out Array<Piece> pieces))
            {
                foreach (var checkPiece in pieces)
                {
                    // Make sure the piece is on the same team
                    if (checkPiece.teamId != piece.teamId)
                    {
                        continue;
                    }
                    // Make sure piece hasn't moved
                    if (checkPiece.timesMoved > 0)
                    {
                        continue;
                    }
                    // If it hasn't moved, make sure it's a rook.
                    if (!checkPiece.IsPiece("rook"))
                    {
                        continue;
                    }
                    
                    // If valid castle, add tag to piece
                    piece.tags.Add("has_castle_action");
                    
                    // If it is a rook, we need to create a move for the castling piece 2 spaces from the King
                    MoveAction newMove = new MoveAction(piece, actionLocation, actionLocation);
                    piece.AddAction(newMove);
                    
                    // then we need to create a move for it 2 back and the current piece and create dependency slides back
                    MoveOther otherMove = new MoveOther(piece, actionLocation, actionLocation - direction, checkPiece.id);
                    piece.AddAction(otherMove);
                    
                    // New move has to be dependent on a slide, whereas the other move jumps over afterward
                    otherMove.AddDependency(newMove);

                    MoveAction prevMove = newMove;
                    for (int j = i - 1; j > 0 ; j--)
                    {
                        Vector2I loc = actionLocation - direction * j;
                        SlideAction newSlide = new SlideAction(piece, actionLocation, checkLocation - direction * j);
                        prevMove.AddDependency(newSlide);
                        piece.AddAction(newSlide);
                        prevMove = newSlide;
                    }
                }
            }
        }
    }

    public override void EndTurn(GameState game, Piece piece)
    {
        // If the piece has the 'has_castle_action' tag, EnableActionUpdates
        if (piece.HasTag("has_castle_action"))
        {
            piece.EnableActionsUpdate();
            piece.tags.Remove("has_castle_action");
        }
    }
}