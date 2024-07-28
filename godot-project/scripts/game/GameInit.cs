using Godot;
using System.Collections.Generic;

public partial class GameController : Node
{
    internal Registry<PieceInfo> pieceInfoRegistry = new Registry<PieceInfo>();
    internal Registry<ActionRuleBase> actionRuleRegistry = new Registry<ActionRuleBase>();
    internal Registry<ValidationRuleBase> validationRuleRegistry = new Registry<ValidationRuleBase>();
    List<string> initialValidationRules = new List<string>();

    public void FullInit()
    {
        InitGameState();
        InitValidationRules();
        InitActionRules();
        InitPieceInfo();

        AddChild(pieceInfoRegistry);
        AddChild(actionRuleRegistry);
        AddChild(validationRuleRegistry);
    }

    public GameState InitGameState()
    {
        // Create the GameState
        currentGameState = new GameState(this);
        currentGameState.Init();
        AddChild(currentGameState);
        
        // Connect the signals
        currentGameState.NewTurn += (newPlayerNum) => EmitSignal(SignalName.NewTurn, newPlayerNum);
        currentGameState.PieceRemoved += (removedPiece) => EmitSignal(SignalName.PieceRemoved, removedPiece);
        currentGameState.EndTurn += () => EmitSignal(SignalName.EndTurn);
        currentGameState.SendNotice += (playerTarget, text) => EmitSignal(SignalName.SendNotice, playerTarget, text);
        
        return currentGameState;
    }

    internal void InitValidationRules()
    {
        // Make sure registry and initial rules are cleared
        validationRuleRegistry.Clear();
        initialValidationRules.Clear();

        // Register Rules
        MakeNewValidationRule("no_team_attack", new NoTeamAttackRule(), true);
        MakeNewValidationRule("no_team_overlap", new NoTeamOverlapRule(), true);
        MakeNewValidationRule("no_enemy_overlap", new NoEnemyOverlapRule(), true);
        MakeNewValidationRule("enemy_attack_allow_overlap", new EnemyAttackAllowOverlapRule(), true);
        MakeNewValidationRule("line_move_stop", new LineMoveStopRule(), true);
        MakeNewValidationRule("inside_board", new InsideBoardRule(), true);
        MakeNewValidationRule("attack_needs_target", new AttackNeedsTargetRule(), true);
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
        MakeNewActionRule("castle", new CastleRule());
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
        MakeNewPieceInfo("king", 2, "king.png").AddActionRule(actionRuleRegistry.GetValue("king_move")).AddActionRule(actionRuleRegistry.GetValue("castle"));
    }

    private void MakeNewValidationRule(string id, ValidationRuleBase newRule, bool makeInitialRule = false)
    {
        newRule.ruleId = id;
        validationRuleRegistry.Register(id, newRule);
        if (makeInitialRule)
        {
            initialValidationRules.Add(id);
        }
        GD.Print($"Made new Validation Rule: {id}");
    }

    private void MakeNewActionRule(string id, ActionRuleBase newRule)
    {
        newRule.ruleId = id;
        actionRuleRegistry.Register(id, newRule);
        GD.Print($"Made new Action Rule: {id}");
    }

    private PieceInfo MakeNewPieceInfo(string id, int initialLevel, string textureLocation = "default.png")
    {
        PieceInfo newInfo = new PieceInfo(id, initialLevel);

        foreach (string ruleId in initialValidationRules)
        {
            newInfo.AddValidationRule(validationRuleRegistry.GetValue(ruleId));
        }

        newInfo.textureLoc = textureLocation;
        pieceInfoRegistry.Register(id, newInfo);
        GD.Print($"Made new Piece Info: {id} (initialLeveL: {initialLevel}, textureLocation: {textureLocation})");
        return newInfo;
    }
}
