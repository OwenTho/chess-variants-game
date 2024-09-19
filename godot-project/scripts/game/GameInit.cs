using Godot;
using System.Collections.Generic;
using Godot.Collections;

public partial class GameController
{
    private bool hasInitBefore;
    internal Registry<PieceInfo> pieceInfoRegistry = new();
    
    internal Registry<ActionRuleBase> actionRuleRegistry = new();
    internal Registry<ActionFactory> actionFactoriesRegistry = new();
    
    internal Registry<ValidationRuleBase> validationRuleRegistry = new();
    internal List<string> initialValidationRules = new();
    
    internal Registry<CardFactory> cardFactoryRegistry = new();

    public CardDeck MajorCardDeck { get; private set; }
    public CardDeck MinorCardDeck { get; private set; }
    public CardFactory ChangePieceFactory { get; private set; }
    
    public void FullInit(bool isServer, int playerCount)
    {
        InitGameState(isServer, playerCount);
        InitValidationRules();
        InitActionRules();
        InitActionFactories();
        InitPieceInfo();
        InitCardFactories();
        InitCardDecks(isServer);
        InitInitialCards();

        if (!hasInitBefore)
        {
            hasInitBefore = true;
            AddChild(pieceInfoRegistry);
            AddChild(actionRuleRegistry);
            AddChild(actionFactoriesRegistry);
            AddChild(validationRuleRegistry);
            AddChild(cardFactoryRegistry);
        }
    }

    public GameState InitGameState(bool needToCheck, int playerCount)
    {
        // Create the GameState
        currentGameState = new GameState(this, playerCount);
        currentGameState.Init(needToCheck);
        AddChild(currentGameState);
        
        // Connect the signals
        currentGameState.TurnStarted += (newPlayerNum) => EmitSignal(SignalName.TurnStarted, newPlayerNum);
        currentGameState.TurnEnded += (oldPlayerNum, newPlayerNum) => EmitSignal(SignalName.TurnEnded, oldPlayerNum, newPlayerNum);
        
        currentGameState.ActionProcessed += (action, piece) => EmitSignal(SignalName.ActionProcessed, action, piece);
        currentGameState.ActionsProcessedAt += (success, actionLocation, piece) => EmitSignal(SignalName.ActionsProcessedAt, success, actionLocation, piece);

        currentGameState.CardAdded += (card) => EmitSignal(SignalName.CardAdded, card);
        currentGameState.CardNotice += (card, notice) => EmitSignal(SignalName.CardNotice, card, notice);
        
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
        MakeNewValidationRule(LonelyPiecesStuckCard.RuleId, new LonelyPiecesStuckRule());
        MakeNewValidationRule("allow_team_attack", new AllowTeamAttackRule());
        MakeNewValidationRule("team_attack_allow_overlap", new TeamAttackAllowOverlapRule());
    }
    
    internal void InitActionRules()
    {
        // Make sure registry is cleared
        actionRuleRegistry.Clear();

        // Register Rules
        // Generic
        MakeNewActionRule(LineMoveId.UpLeft, new UpLeftLineMoveRule());
        MakeNewActionRule(LineMoveId.Up, new UpLineMoveRule());
        MakeNewActionRule(LineMoveId.UpRight, new UpRightLineMoveRule());
        
        MakeNewActionRule(LineMoveId.Left, new LeftLineMoveRule());
        MakeNewActionRule(LineMoveId.Right, new RightLineMoveRule());
        
        MakeNewActionRule(LineMoveId.DownLeft, new DownLeftLineMoveRule());
        MakeNewActionRule(LineMoveId.Down, new DownLineMoveRule());
        MakeNewActionRule(LineMoveId.DownRight, new DownRightLineMoveRule());
        
        MakeNewActionRule("nothing", new NothingMoveRule());
        
        // Piece Specific
        MakeNewActionRule("pawn_move", new PawnMoveRule());
        MakeNewActionRule("knight_move", new KnightMoveRule());
        MakeNewActionRule("king_move", new KingMoveRule());
        MakeNewActionRule("castle", new CastleRule());
        
        // Piece Specific - Custom
        MakeNewActionRule("warp_bishop_move", new WarpBishopMoveRule());
    }

    internal void InitActionFactories()
    {
        // Make sure registry is cleared
        actionFactoriesRegistry.Clear();
        
        // Register factories for the actions
        RegisterNewAction<MoveAction>(ActionIds.Move);
        RegisterNewAction<AttackAction>(ActionIds.Attack);
        RegisterNewAction<MoveOther>(ActionIds.MoveOther);
        RegisterNewAction<PawnMoveAction>(ActionIds.PawnMove);

        RegisterNewAction<NothingAction>(ActionIds.Nothing);
    }

    internal void InitPieceInfo()
    {
        // Make sure registry is cleared
        pieceInfoRegistry.Clear();

        // Register Piece Info
        MakeNewPieceInfo("pawn", "Pawn", "pawn.png").AddActionRule(actionRuleRegistry.GetValue("pawn_move"));
        
        PieceInfo rookInfo = MakeNewPieceInfo("rook", "Rook", "rook.png");
        rookInfo.AddActionRule(GetActionRule(LineMoveId.Left), 7).AddActionRule(GetActionRule(LineMoveId.Up), 7);
        rookInfo.AddActionRule(GetActionRule(LineMoveId.Right), 7).AddActionRule(GetActionRule(LineMoveId.Down), 7);
        MakeNewPieceInfo("knight", "Knight", "knight.png").AddActionRule(actionRuleRegistry.GetValue("knight_move"), 3);
        
        PieceInfo bishopInfo = MakeNewPieceInfo("bishop", "Bishop", "bishop.png");
        bishopInfo.AddActionRule(GetActionRule(LineMoveId.UpLeft), 7).AddActionRule(GetActionRule(LineMoveId.UpRight), 7);
        bishopInfo.AddActionRule(GetActionRule(LineMoveId.DownLeft), 7).AddActionRule(GetActionRule(LineMoveId.DownRight), 7);
        
        PieceInfo queenInfo = MakeNewPieceInfo("queen", "Queen", "queen.png");
        queenInfo.AddActionRule(GetActionRule(LineMoveId.Left), 7).AddActionRule(GetActionRule(LineMoveId.Up), 7);
        queenInfo.AddActionRule(GetActionRule(LineMoveId.Right), 7).AddActionRule(GetActionRule(LineMoveId.Down), 7);
        queenInfo.AddActionRule(GetActionRule(LineMoveId.UpLeft), 7).AddActionRule(GetActionRule(LineMoveId.UpRight), 7);
        queenInfo.AddActionRule(GetActionRule(LineMoveId.DownLeft), 7).AddActionRule(GetActionRule(LineMoveId.DownRight), 7);

        PieceInfo kingInfo = MakeNewPieceInfo("king", "King", "king.png");
        kingInfo.AddActionRule(GetActionRule(LineMoveId.Left), 1).AddActionRule(GetActionRule(LineMoveId.Right), 1);
        kingInfo.AddActionRule(GetActionRule(LineMoveId.Up), 1).AddActionRule(GetActionRule(LineMoveId.Down), 1);
        kingInfo.AddActionRule(GetActionRule(LineMoveId.UpLeft), 1, true)
            .AddActionRule(GetActionRule(LineMoveId.UpRight), 1, true);
        kingInfo.AddActionRule(GetActionRule(LineMoveId.DownLeft), 1, true)
            .AddActionRule(GetActionRule(LineMoveId.DownRight), 1, true);
        kingInfo.AddActionRule(actionRuleRegistry.GetValue("castle"));
        
        // Register custom Piece Info
        MakeNewPieceInfo("warp_bishop", "Warp Bishop", "warp_bishop.png").AddActionRule(GetActionRule("warp_bishop_move"), 7);
        MakeNewPieceInfo("rock", "Rock", "rock.png").AddActionRule(GetActionRule("nothing"));
    }

    internal void InitCardFactories()
    {
        // Make sure registry is cleared
        cardFactoryRegistry.Clear();
        
        // Register Card Factories
        AddNewCardFactory(CardIds.Shapeshift, new SimpleCardFactory<ShapeshiftCard>());
        // AddNewCardFactory(CardIds.SinglePieceArmy, new SinglePieceArmyCardFactory());
        AddNewCardFactory(CardIds.PawnArmy, new SimpleCardFactory<PawnArmyCard>());
        AddNewCardFactory(CardIds.Shuffle, new SimpleCardFactory<ShuffleCard>());
        AddNewCardFactory(CardIds.LonelyPieces, new SimpleCardFactory<LonelyPiecesStuckCard>(), true);
        AddNewCardFactory(CardIds.FriendlyFire, new SimpleCardFactory<FriendlyFireCard>(), true);
        AddNewCardFactory(CardIds.BiggerBoard, new SimpleCardFactory<BiggerBoardCard>());
        AddNewCardFactory(CardIds.LevelUp, new SimpleCardFactory<LevelUpCard>());
        
        // Minor Cards
        // Rules
        AddNewCardFactory(CardIds.Promotion, new PomotionCardFactory());
        
        // Piece
        ChangePieceFactory = AddNewCardFactory(CardIds.ChangePiece, new ChangePieceCardFactory(), true, false);
        
        // Space
    }

    internal void InitCardDecks(bool isServer)
    {
        // Dispose
        if (MajorCardDeck != null)
        {
            if (IsInstanceValid(MajorCardDeck))
            {
                MajorCardDeck.Free();
            }
            MajorCardDeck = null;
        }

        if (MinorCardDeck != null)
        {
            if (IsInstanceValid(MinorCardDeck))
            {
                MinorCardDeck.Free();
            }
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
        
        MajorCardDeck.AddCard(GetCardFactory(CardIds.Shapeshift));
        MajorCardDeck.AddCard(GetCardFactory(CardIds.PawnArmy));
        MajorCardDeck.AddCard(GetCardFactory(CardIds.Shuffle));
        MajorCardDeck.AddCard(GetCardFactory(CardIds.LonelyPieces));
        MajorCardDeck.AddCard(GetCardFactory(CardIds.FriendlyFire));
        MajorCardDeck.AddCard(GetCardFactory(CardIds.BiggerBoard));
        MajorCardDeck.AddCard(GetCardFactory(CardIds.LevelUp));
        
        // Set up the Decks
        MinorCardDeck = new CardDeck();
        MinorCardDeck.RemoveCards = false;
        MinorCardDeck.AddCard(GetCardFactory(CardIds.Promotion), CardWeights.Common);
        AddChild(MinorCardDeck);
    }

    internal void InitInitialCards()
    {
        // TODO: Make this server-only, and send all initial cards when the game starts.
        PromotionCard newCard = (PromotionCard) cardFactoryRegistry.GetValue("minor_promotion").CreateNewBlankCard(currentGameState);
        newCard.fromPiece = "pawn";
        newCard.toPiece = new Array<string> { "knight", "bishop", "rook", "queen" };
        AddCard(newCard);
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

    public bool TryGetActionRule(string id, out ActionRuleBase rule)
    {
        return actionRuleRegistry.TryGetValue(id, out rule);
    }
    
    public ActionRuleBase GetActionRule(string id)
    {
        return actionRuleRegistry.GetValue(id);
    }
    
    private void MakeNewActionRule(string id, ActionRuleBase newRule, int defaultLevel = 1)
    {
        newRule.ruleId = id;
        newRule.defaultLevel = defaultLevel;
        actionRuleRegistry.Register(id, newRule);
        GD.Print($"Made new Action Rule: {id}");
    }

    private ActionFactory RegisterNewAction<T>(string id) where T : ActionBase, new()
    {
        // Update the Action's actionId
        SimpleActionFactory<T> newFactory = new SimpleActionFactory<T>();
        newFactory.actionId = id;
        actionFactoriesRegistry.Register(id, newFactory);
        GD.Print($"Made new Action Factory for {typeof(T).Name}: {id}");
        return newFactory;
    }

    private PieceInfo MakeNewPieceInfo(string id, string displayName = null, string textureLocation = "default.png", int defaultLevel = 0)
    {
        PieceInfo newInfo = new PieceInfo(id, displayName, defaultLevel);

        foreach (string ruleId in initialValidationRules)
        {
            newInfo.AddValidationRule(validationRuleRegistry.GetValue(ruleId));
        }

        newInfo.textureLoc = textureLocation;
        pieceInfoRegistry.Register(id, newInfo);
        GD.Print($"Made new Piece Info: {id} (defaultLevel: {defaultLevel}, textureLocation: {textureLocation})");
        return newInfo;
    }

    private CardFactory AddNewCardFactory(string id, CardFactory newFactory, bool serverOnly = false, bool displayCard = true, bool immediateUse = false)
    {
        newFactory.cardId = id;
        newFactory.serverOnly = serverOnly;
        newFactory.immediateUse = immediateUse;
        newFactory.displayCard = displayCard;
        cardFactoryRegistry.Register(id, newFactory);
        return newFactory;
    }

    public CardFactory GetCardFactory(string id)
    {
        return cardFactoryRegistry.GetValue(id);
    }

    public bool TryGetCardFactory(string id, out CardFactory factory)
    {
        return cardFactoryRegistry.TryGetValue(id, out factory);
    }
}
