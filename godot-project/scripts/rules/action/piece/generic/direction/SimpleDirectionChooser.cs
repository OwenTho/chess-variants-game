public class SimpleDirectionChooser : LineDirectionChooserBase {
  public readonly RelativePieceDirection[] Dirs;

  public SimpleDirectionChooser(RelativePieceDirection[] dirs) {
    Dirs = dirs;
  }

  public SimpleDirectionChooser(RelativePieceDirection dir) : this(new[] { dir }) {
  }

  public override RelativePieceDirection[] GetDirs(GameState game, Piece piece) {
    return Dirs;
  }
}