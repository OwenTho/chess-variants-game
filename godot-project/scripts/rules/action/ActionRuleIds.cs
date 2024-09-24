
public static class ActionRuleIds
{
    // Generic
    public const string Nothing = "nothing";

    // Line rules
    public static class Line
    {
        public static class Move
        {
            public const string ForwardLeft = "slide_move_front_left";
            public const string Forward = "slide_move_front";
            public const string ForwardRight = "slide_move_front_right";

            public const string Left = "slide_move_left";
            public const string Right = "slide_move_right";

            public const string BackwardLeft = "slide_move_back_left";
            public const string Backward = "slide_move_back";
            public const string BackwardRight = "slide_move_back_right";
        }
        
        public static class MoveAttack
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

        public static class SlideAttack
        {
            public const string ForwardLeft = "slide_attack_front_left";
            public const string Forward = "slide_attack_front";
            public const string ForwardRight = "slide_attack_front_right";

            public const string Left = "slide_attack_left";
            public const string Right = "slide_attack_right";

            public const string BackwardLeft = "slide_attack_back_left";
            public const string Backward = "slide_attack_back";
            public const string BackwardRight = "slide_attack_back_right";
        }
    }
    
    
    // Piece Specific
    public const string PawnMove = "pawn_move";
    public const string KnightMove = "knight_move";
    public const string KingMove = "king_move";
    
    public const string Castle = "castle";
    
    // Unique pieces
    public const string WarpBishopMove = "warp_bishop_move";
    public const string PrawnMove = "prawn_move";
}