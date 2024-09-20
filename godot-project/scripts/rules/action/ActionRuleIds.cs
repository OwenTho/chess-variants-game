
public static class ActionRuleIds
{
    // Generic
    public const string Nothing = "nothing";

    public static class Line
    {
        public const string ForwardLeft = "line_front_left";
        public const string Forward = "line_front";
        public const string ForwardRight = "line_front_right";

        public const string Left = "line_left";
        public const string Right = "line_right";
            
        public const string BackwardLeft = "line_back_left";
        public const string Backward = "line_back";
        public const string BackwardRight = "line_back_right";
    }
    
    
    // Piece Specific
    public const string PawnMove = "pawn_move";
    public const string KnightMove = "knight_move";
    public const string KingMove = "king_move";
    
    public const string Castle = "castle";
    
    // Unique pieces
    public const string WarpBishopMove = "warp_bishop_move";
}