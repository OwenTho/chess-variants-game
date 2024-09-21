using Godot;

public abstract partial class RelativeLineRuleBase : LineRuleBase
{
    public readonly LineDirectionChooserBase DirectionChooser;

    public RelativeLineRuleBase(LineDirectionChooserBase chooser)
    {
        DirectionChooser = chooser;
    }
    
    public RelativeLineRuleBase(RelativePieceDirection[] dirs)
    {
        DirectionChooser = new SimpleDirectionChooser(dirs);
    }
    
    public RelativeLineRuleBase(RelativePieceDirection dir) : this(new []{ dir })
    {
        
    }
    
    
    
    public override Vector2I[] GetDirs(GameState game, Piece piece)
    {
        RelativePieceDirection[] directions = DirectionChooser.GetDirs(game, piece);
        
        Vector2I[] returnDirs = new Vector2I[directions.Length];
        for (var i = 0 ; i < directions.Length ; i++)
        {
            returnDirs[i] = directions[i].FromDir(piece.forwardDirection).AsVector();
        }
        return returnDirs;
    }
}