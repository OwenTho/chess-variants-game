using Godot;
using System.Collections.Generic;


public partial class GameController : Node
{
    
    internal void InitRules()
    {
        // Make sure registry is cleared
        ruleRegistry.Clear();

        // Register Rules
        MakeNewRule("pawn_move", new PawnMoveRule());
        MakeNewRule("rook_move", new RookMoveRule());
        MakeNewRule("knight_move", new KnightMoveRule());
        MakeNewRule("bishop_move", new BishopMoveRule());
        MakeNewRule("queen_move", new QueenMoveRule());
        MakeNewRule("king_move", new KingMoveRule());

    }

    internal void InitPieceInfo()
    {
        // Make sure registry is cleared
        pieceInfoRegistry.Clear();

        // Register Piece Info
        MakeNewPieceInfo("pawn", 1, "pawn.png").AddRule(ruleRegistry.GetValue("pawn_move"));
        MakeNewPieceInfo("rook", 7, "rook.png").AddRule(ruleRegistry.GetValue("rook_move"));
        MakeNewPieceInfo("knight", 4, "knight.png").AddRule(ruleRegistry.GetValue("knight_move"));
        MakeNewPieceInfo("bishop", 7, "bishop.png").AddRule(ruleRegistry.GetValue("bishop_move"));
        MakeNewPieceInfo("queen", 7, "queen.png").AddRule(ruleRegistry.GetValue("queen_move"));
        MakeNewPieceInfo("king", 2, "king.png").AddRule(ruleRegistry.GetValue("king_move"));
    }

    private void MakeNewRule(string id, RuleBase newRule)
    {
        newRule.ruleId = id;
        ruleRegistry.Register(id, newRule);
        GD.Print($"Made new Rule: {id}");
    }

    private PieceInfo MakeNewPieceInfo(string id, int initialLevel, string textureLocation = "pawn.png")
    {
        /*List<PieceRule> rules = new List<PieceRule>();
        foreach (string ruleId in initialRules)
        {
            if (ruleRegistry.TryGetValue(ruleId, out RuleBase rule))
            {
                rules.Add(new PieceRule(rule));
            }
            else
            {
                GD.PushWarning($"Could not find rule {ruleId} for {id}, so it has been ignored.");
            }
        }*/
        PieceInfo newInfo = new PieceInfo(id, initialLevel);
        newInfo.textureLoc = textureLocation;
        pieceInfoRegistry.Register(id, newInfo);
        GD.Print($"Made new Piece Info: {id} (initialLeveL: {initialLevel}, textureLocation: {textureLocation})");
        return newInfo;
    }
}
