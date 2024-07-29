
using ChessVariantsGame.scripts.actions.movement;
using Godot;
using Godot.Collections;

public partial class CastleRule : ActionRuleBase
{
    private const int MIN_DIST = 1;
    
    public override void AddPossibleActions(GameState game, Piece piece)
    {
        // If the piece has moved, ignore
        if (piece.timesMoved > 0)
        {
            return;
        }
        // Check for castle, left and right
        CheckCastle(game, piece, Vector2I.Left);
        CheckCastle(game, piece, Vector2I.Right);
    }

    private void CheckCastle(GameState game, Piece piece, Vector2I direction)
    {
        // TODO: Use level instead of repeating 4 times
        // Repeat up to 4 away, from a distance of 2.
        Vector2I actionLocation = piece.cell.pos + direction * (MIN_DIST - 1);
        for (int i = MIN_DIST; i < 5; i++)
        {
            Vector2I checkLocation = piece.cell.pos + direction * i;
            
            // Check the location for a piece
            if (game.TryGetPiecesAt(checkLocation.X, checkLocation.Y, out Array<Piece> pieces))
            {
                foreach (var checkPiece in pieces)
                {
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
                    
                    // If it is a rook, we need to create a move for the castling piece 1 space before
                    MoveAction newMove = new MoveAction(piece, actionLocation, actionLocation);
                    piece.AddAction(newMove);
                    
                    // then we need to create a move for it 2 back and the current piece and create dependency slides back
                    MoveOther otherMove = new MoveOther(piece, actionLocation, actionLocation - direction, checkPiece);
                    piece.AddAction(otherMove);
                    
                    // New move has to be dependent on a slide, whereas the other move jumps over afterward
                    otherMove.AddDependency(newMove);

                    MoveAction prevMove = newMove;
                    for (int j = i - 2; j > 0 ; j--)
                    {
                        SlideAction newSlide = new SlideAction(piece, actionLocation, actionLocation - direction * j);
                        prevMove.AddDependency(newSlide);
                        piece.AddAction(newSlide);
                        prevMove = newSlide;
                    }
                }
            }
            
            // Update the action location, as it moves one over
            actionLocation = checkLocation;
        }
    }
}