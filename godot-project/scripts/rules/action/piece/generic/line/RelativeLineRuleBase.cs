using Godot;

public abstract partial class RelativeLineRuleBase : LineRuleBase {
  public readonly LineDirectionChooserBase DirectionChooser;

  public RelativeLineRuleBase(LineDirectionChooserBase chooser) {
    DirectionChooser = chooser;
  }

  public RelativeLineRuleBase(RelativePieceDirection[] dirs) {
    DirectionChooser = new SimpleDirectionChooser(dirs);
  }

  public RelativeLineRuleBase(RelativePieceDirection dir) : this(new[] { dir }) {
  }


  public override Vector2I[] GetDirs(GameState game, Piece piece) {
    RelativePieceDirection[] directions = DirectionChooser.GetDirs(game, piece);

    var returnDirs = new Vector2I[directions.Length];
    for (int i = 0; i < directions.Length; i++) {
      returnDirs[i] = directions[i].FromDir(piece.ForwardDirection).AsVector();
    }

    return returnDirs;
  }
}