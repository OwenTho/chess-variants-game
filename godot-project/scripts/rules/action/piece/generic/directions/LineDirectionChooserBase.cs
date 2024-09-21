
using Godot;

public abstract class LineDirectionChooserBase
{
    public abstract RelativePieceDirection[] GetDirs(GameState game, Piece piece);
}