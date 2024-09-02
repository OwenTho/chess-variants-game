﻿using Godot;
using System.Collections.Generic;

public partial class GameController
{
    internal Registry<PieceInfo> pieceInfoRegistry = new Registry<PieceInfo>();
    internal Registry<ActionRuleBase> actionRuleRegistry = new Registry<ActionRuleBase>();
    internal Registry<ValidationRuleBase> validationRuleRegistry = new Registry<ValidationRuleBase>();
    List<string> initialValidationRules = new List<string>();
    internal Registry<CardFactory> cardFactoryRegistry = new Registry<CardFactory>();
    private bool hasInitBefore = false;

    public CardDeck MajorCardDeck { get; private set; }
    public CardDeck MinorCardDeck { get; private set; }
    
    public void FullInit(bool isServer)
    {
        InitGameState(isServer);
        InitValidationRules();
        InitActionRules();
        InitPieceInfo();
        InitCardFactories();
        InitCardDecks(isServer);

        if (!hasInitBefore)
        {
            hasInitBefore = true;
            AddChild(pieceInfoRegistry);
            AddChild(actionRuleRegistry);
            AddChild(validationRuleRegistry);
            AddChild(cardFactoryRegistry);
        }
    }

    public GameState InitGameState(bool needToCheck)
    {
        // Create the GameState
        currentGameState = new GameState(this, 2);
        currentGameState.Init(needToCheck);
        AddChild(currentGameState);
        
        // Connect the signals
        currentGameState.NewTurn += (newPlayerNum) => EmitSignal(SignalName.NewTurn, newPlayerNum);
        currentGameState.ActionProcessed += (success, actionLocation, piece) => EmitSignal(SignalName.ActionProcessed, success, actionLocation, piece);
        currentGameState.EndTurn += () => EmitSignal(SignalName.EndTurn);
        
        currentGameState.PlayerLost += (playerNum) => EmitSignal(SignalName.PlayerLost, playerNum);
        currentGameState.GameStalemate += (stalematePlayer) => EmitSignal(SignalName.GameStalemate, stalematePlayer);
        
        currentGameState.PieceRemoved += (removedPiece, attackerPiece) => EmitSignal(SignalName.PieceRemoved, removedPiece, attackerPiece);
        
        currentGameState.SendNotice += (playerTarget, text) => EmitSignal(SignalName.SendNotice, playerTarget, text);

        currentGameState.UpperBoundChanged += (newBound) => EmitSignal(SignalName.UpperBoundChanged, newBound);
        currentGameState.LowerBoundChanged += (newBound) => EmitSignal(SignalName.LowerBoundChanged, newBound);

        
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
        
        // Register Major Card Rules
        MakeNewValidationRule("lonely_piece_card", new LonelyPiecesStuckRule(), false);
        MakeNewValidationRule("allow_team_attack", new AllowTeamAttackRule(), false);
        MakeNewValidationRule("team_attack_allow_overlap", new TeamAttackAllowOverlapRule(), false);
    }
    
    internal void InitActionRules()
    {
        // Make sure registry is cleared
        actionRuleRegistry.Clear();

        // Register Rules
        MakeNewActionRule("pawn_move", new PawnMoveRule());
        MakeNewActionRule("rook_move", new RookMoveRule(), 7);
        MakeNewActionRule("knight_move", new KnightMoveRule());
        MakeNewActionRule("bishop_move", new BishopMoveRule(), 7);
        MakeNewActionRule("queen_move", new QueenMoveRule(), 7);
        MakeNewActionRule("king_move", new KingMoveRule());
        MakeNewActionRule("castle", new CastleRule());
    }

    internal void InitPieceInfo()
    {
        // Make sure registry is cleared
        pieceInfoRegistry.Clear();

        // Register Piece Info
        MakeNewPieceInfo("pawn", "pawn.png").AddActionRule(actionRuleRegistry.GetValue("pawn_move"));
        MakeNewPieceInfo("rook", "rook.png").AddActionRule(actionRuleRegistry.GetValue("rook_move"));
        MakeNewPieceInfo("knight", "knight.png").AddActionRule(actionRuleRegistry.GetValue("knight_move"));
        MakeNewPieceInfo("bishop", "bishop.png").AddActionRule(actionRuleRegistry.GetValue("bishop_move"));
        MakeNewPieceInfo("queen",  "queen.png").AddActionRule(actionRuleRegistry.GetValue("queen_move"));
        MakeNewPieceInfo("king", "king.png").AddActionRule(actionRuleRegistry.GetValue("king_move")).AddActionRule(actionRuleRegistry.GetValue("castle"));
    }

    internal void InitCardFactories()
    {
        // Make sure registry is cleared
        cardFactoryRegistry.Clear();
        
        // Register Card Factories
        AddNewFactory("major_shapeshift", new SimpleCardFactory<ShapeshiftCard>());
        AddNewFactory("major_single_piece_army", new SinglePieceArmyCardFactory());
        AddNewFactory("major_shuffle", new SimpleCardFactory<ShuffleCard>());
        AddNewFactory("major_lonely_pieces_stuck", new SimpleCardFactory<LonelyPiecesStuckCard>());
        AddNewFactory("major_friendly_fire", new SimpleCardFactory<FriendlyFireCard>());
        AddNewFactory("major_bigger_board", new SimpleCardFactory<BiggerBoardCard>());
    }

    internal void InitCardDecks(bool isServer)
    {
        // Dispose
        if (MajorCardDeck != null)
        {
            MajorCardDeck.Free();
            MajorCardDeck = null;
        }

        if (MinorCardDeck != null)
        {
            MinorCardDeck.Free();
            MinorCardDeck = null;
        }
        
        // If not the server, no need to create the card decks
        if (!isServer)
        {
            return;
        }
        
        // Set up the cards
        MajorCardDeck = new CardDeck();
        AddChild(MajorCardDeck);
        
        MajorCardDeck.AddCard(cardFactoryRegistry.GetValue("major_shapeshift"));
        MajorCardDeck.AddCard(cardFactoryRegistry.GetValue("major_single_piece_army"));
        MajorCardDeck.AddCard(cardFactoryRegistry.GetValue("major_shuffle"));
        MajorCardDeck.AddCard(cardFactoryRegistry.GetValue("major_lonely_pieces_stuck"));
        MajorCardDeck.AddCard(cardFactoryRegistry.GetValue("major_friendly_fire"));
        MajorCardDeck.AddCard(cardFactoryRegistry.GetValue("major_bigger_board"));
        
        // Set up the Decks
        MinorCardDeck = new CardDeck();
        AddChild(MinorCardDeck);
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

    private void MakeNewActionRule(string id, ActionRuleBase newRule, int defaultLevel = 1)
    {
        newRule.ruleId = id;
        newRule.defaultLevel = defaultLevel;
        actionRuleRegistry.Register(id, newRule);
        GD.Print($"Made new Action Rule: {id}");
    }

    private PieceInfo MakeNewPieceInfo(string id, string textureLocation = "default.png", int defaultLevel = 0)
    {
        PieceInfo newInfo = new PieceInfo(id, initialLevel);
        PieceInfo newInfo = new PieceInfo(id, defaultLevel);

        foreach (string ruleId in initialValidationRules)
        {
            newInfo.AddValidationRule(validationRuleRegistry.GetValue(ruleId));
        }

        newInfo.textureLoc = textureLocation;
        pieceInfoRegistry.Register(id, newInfo);
        GD.Print($"Made new Piece Info: {id} (defaultLevel: {defaultLevel}, textureLocation: {textureLocation})");
        return newInfo;
    }

    private void AddNewFactory(string id, CardFactory newFactory)
    {
        newFactory.cardId = id;
        cardFactoryRegistry.Register(id, newFactory);
    }
}
