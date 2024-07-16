using Godot;
using System.Collections.Generic;


public partial class GameController : Node
{
    Registry<PieceInfo> pieceInfoRegistry = new Registry<PieceInfo>();
    Registry<ActionRuleBase> actionRuleRegistry = new Registry<ActionRuleBase>();
    Registry<ValidationRuleBase> validationRuleRegistry = new Registry<ValidationRuleBase>();
    internal void InitValidationRules()
    {
        // Make sure registry is cleared
        validationRuleRegistry.Clear();

        // Register Rules
        MakeNewValidationRule("no_team_attack", new NoTeamAttackRule());
        MakeNewValidationRule("no_team_overlap", new NoTeamOverlapRule());
        MakeNewValidationRule("inside_board", new InsideBoardRule());
    }
    
    internal void InitActionRules()
    {
        // Make sure registry is cleared
        actionRuleRegistry.Clear();

        // Register Rules
        MakeNewActionRule("pawn_move", new PawnMoveRule());
        MakeNewActionRule("rook_move", new RookMoveRule());
        MakeNewActionRule("knight_move", new KnightMoveRule());
        MakeNewActionRule("bishop_move", new BishopMoveRule());
        MakeNewActionRule("queen_move", new QueenMoveRule());
        MakeNewActionRule("king_move", new KingMoveRule());
    }

    internal void InitPieceInfo()
    {
        // Make sure registry is cleared
        pieceInfoRegistry.Clear();

        // Register Piece Info
        MakeNewPieceInfo("pawn", 1, "pawn.png").AddActionRule(actionRuleRegistry.GetValue("pawn_move"));
        MakeNewPieceInfo("rook", 7, "rook.png").AddActionRule(actionRuleRegistry.GetValue("rook_move"));
        MakeNewPieceInfo("knight", 4, "knight.png").AddActionRule(actionRuleRegistry.GetValue("knight_move"));
        MakeNewPieceInfo("bishop", 7, "bishop.png").AddActionRule(actionRuleRegistry.GetValue("bishop_move"));
        MakeNewPieceInfo("queen", 7, "queen.png").AddActionRule(actionRuleRegistry.GetValue("queen_move"));
        MakeNewPieceInfo("king", 2, "king.png").AddActionRule(actionRuleRegistry.GetValue("king_move"));
    }

    private void MakeNewValidationRule(string id, ValidationRuleBase newRule)
    {
        newRule.ruleId = id;
        validationRuleRegistry.Register(id, newRule);
        GD.Print($"Made new Rule: {id}");
    }

    private void MakeNewActionRule(string id, ActionRuleBase newRule)
    {
        newRule.ruleId = id;
        actionRuleRegistry.Register(id, newRule);
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

        newInfo.AddValidationRule(validationRuleRegistry.GetValue("no_team_attack"));
        newInfo.AddValidationRule(validationRuleRegistry.GetValue("no_team_overlap"));
        newInfo.AddValidationRule(validationRuleRegistry.GetValue("inside_board"));

        newInfo.textureLoc = textureLocation;
        pieceInfoRegistry.Register(id, newInfo);
        GD.Print($"Made new Piece Info: {id} (initialLeveL: {initialLevel}, textureLocation: {textureLocation})");
        return newInfo;
    }
}
