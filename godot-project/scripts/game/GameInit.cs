using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class GameController {
  internal Registry<ActionFactory> ActionFactoriesRegistry = new();

  internal Registry<ActionRuleBase> ActionRuleRegistry = new();

  internal Registry<CardFactory> CardFactoryRegistry = new();
  private bool _hasInitBefore;

  internal List<string> InitialValidationRules = new();

  // TODO: PieceInfo should instead use a Factory
  internal Registry<PieceInfo> PieceInfoRegistry = new();

  internal Registry<ValidationRuleBase> ValidationRuleRegistry = new();

  public CardDeck MajorCardDeck { get; private set; }
  public CardDeck MinorCardDeck { get; private set; }
  public CardFactory ChangePieceFactory { get; private set; }

  public void FullInit(bool isServer, int playerCount) {
    InitGameState(isServer, playerCount);
    InitValidationRules();
    InitActionRules();
    InitActionFactories();
    InitPieceInfo();
    InitCardFactories();
    InitCardDecks(isServer);
    InitInitialCards();

    if (!_hasInitBefore) {
      _hasInitBefore = true;
      AddChild(PieceInfoRegistry);
      AddChild(ActionRuleRegistry);
      AddChild(ActionFactoriesRegistry);
      AddChild(ValidationRuleRegistry);
      AddChild(CardFactoryRegistry);
    }
  }

  public GameState InitGameState(bool needToCheck, int playerCount) {
    // Create the GameState
    CurrentGameState = new GameState(this, playerCount);
    CurrentGameState.Init(needToCheck);
    AddChild(CurrentGameState);

    // Connect the signals
    CurrentGameState.TurnStarted +=
      newPlayerNum => EmitSignal(SignalName.TurnStarted, newPlayerNum);
    CurrentGameState.TurnEnded += (oldPlayerNum, newPlayerNum) =>
      EmitSignal(SignalName.TurnEnded, oldPlayerNum, newPlayerNum);

    CurrentGameState.ActionProcessed += (action, piece) =>
      EmitSignal(SignalName.ActionProcessed, action, piece);
    CurrentGameState.ActionsProcessedAt += (success, actionLocation, piece) =>
      EmitSignal(SignalName.ActionsProcessedAt, success, actionLocation, piece);

    CurrentGameState.CardAdded += card => EmitSignal(SignalName.CardAdded, card);
    CurrentGameState.CardNotice +=
      (card, notice) => EmitSignal(SignalName.CardNotice, card, notice);

    CurrentGameState.PlayerLost += playerNum => EmitSignal(SignalName.PlayerLost, playerNum);
    CurrentGameState.GameStalemate +=
      stalematePlayer => EmitSignal(SignalName.GameStalemate, stalematePlayer);

    CurrentGameState.PieceRemoved += (removedPiece, attackerPiece) =>
      EmitSignal(SignalName.PieceRemoved, removedPiece, attackerPiece);

    CurrentGameState.SendNotice += (playerTarget, text) =>
      EmitSignal(SignalName.SendNotice, playerTarget, text);

    CurrentGameState.UpperBoundChanged +=
      newBound => EmitSignal(SignalName.UpperBoundChanged, newBound);
    CurrentGameState.LowerBoundChanged +=
      newBound => EmitSignal(SignalName.LowerBoundChanged, newBound);


    return CurrentGameState;
  }

  internal void InitValidationRules() {
    // Make sure registry and initial rules are cleared
    ValidationRuleRegistry.Clear();
    InitialValidationRules.Clear();

    // Register Rules
    MakeNewValidationRule(ValidationRuleIds.NoTeamAttack, new NoTeamAttackRule(), true);
    MakeNewValidationRule(ValidationRuleIds.NoTeamOverlap, new NoTeamOverlapRule(), true);
    MakeNewValidationRule(ValidationRuleIds.NoEnemyOverlap, new NoEnemyOverlapRule(), true);
    MakeNewValidationRule(ValidationRuleIds.EnemyAttackAllowOverlap,
      new EnemyAttackAllowOverlapRule(), true);
    MakeNewValidationRule(ValidationRuleIds.LineMoveStop, new LineMoveStopRule(), true);
    MakeNewValidationRule(ValidationRuleIds.InsideBoard, new InsideBoardRule(), true);
    MakeNewValidationRule(ValidationRuleIds.AttackNeedsTarget, new AttackNeedsTargetRule(), true);

    MakeNewValidationRule(ValidationRuleIds.Piece.Invincible, new InvinciblePieceRule(), true);

    // Register Piece rules
    MakeNewValidationRule(ValidationRuleIds.Piece.BlockadeNoAttack, new BlockadeNoAttackRule());

    // Register Major Card Rules
    MakeNewValidationRule(LonelyPiecesStuckCard.RuleId, new LonelyPiecesStuckRule());
    MakeNewValidationRule(ValidationRuleIds.Counters.AllowTeamAttack, new AllowTeamAttackRule());
    MakeNewValidationRule(ValidationRuleIds.Counters.TeamAttackAllowOverlap,
      new TeamAttackAllowOverlapRule());
  }

  internal void InitActionRules() {
    // Make sure registry is cleared
    ActionRuleRegistry.Clear();

    // Register Rules

    // Line
    // Move
    MakeNewActionRule(ActionRuleIds.Line.Move.ForwardLeft,
      new LineMoveRule(RelativePieceDirection.ForwardLeft));
    MakeNewActionRule(ActionRuleIds.Line.Move.Forward,
      new LineMoveRule(RelativePieceDirection.Forward));
    MakeNewActionRule(ActionRuleIds.Line.Move.ForwardRight,
      new LineMoveRule(RelativePieceDirection.ForwardRight));

    MakeNewActionRule(ActionRuleIds.Line.Move.Left, new LineMoveRule(RelativePieceDirection.Left));
    MakeNewActionRule(ActionRuleIds.Line.Move.Right,
      new LineMoveRule(RelativePieceDirection.Right));

    MakeNewActionRule(ActionRuleIds.Line.Move.BackwardLeft,
      new LineMoveRule(RelativePieceDirection.BackwardLeft));
    MakeNewActionRule(ActionRuleIds.Line.Move.Backward,
      new LineMoveRule(RelativePieceDirection.Backward));
    MakeNewActionRule(ActionRuleIds.Line.Move.BackwardRight,
      new LineMoveRule(RelativePieceDirection.BackwardRight));

    // Move -> Attack
    MakeNewActionRule(ActionRuleIds.Line.MoveAttack.ForwardLeft,
      new LineMoveAttackRule(RelativePieceDirection.ForwardLeft));
    MakeNewActionRule(ActionRuleIds.Line.MoveAttack.Forward,
      new LineMoveAttackRule(RelativePieceDirection.Forward));
    MakeNewActionRule(ActionRuleIds.Line.MoveAttack.ForwardRight,
      new LineMoveAttackRule(RelativePieceDirection.ForwardRight));

    MakeNewActionRule(ActionRuleIds.Line.MoveAttack.Left,
      new LineMoveAttackRule(RelativePieceDirection.Left));
    MakeNewActionRule(ActionRuleIds.Line.MoveAttack.Right,
      new LineMoveAttackRule(RelativePieceDirection.Right));

    MakeNewActionRule(ActionRuleIds.Line.MoveAttack.BackwardLeft,
      new LineMoveAttackRule(RelativePieceDirection.BackwardLeft));
    MakeNewActionRule(ActionRuleIds.Line.MoveAttack.Backward,
      new LineMoveAttackRule(RelativePieceDirection.Backward));
    MakeNewActionRule(ActionRuleIds.Line.MoveAttack.BackwardRight,
      new LineMoveAttackRule(RelativePieceDirection.BackwardRight));

    // Slide -> Move + Attack
    MakeNewActionRule(ActionRuleIds.Line.SlideAttack.ForwardLeft,
      new SlideAttackMoveRule(RelativePieceDirection.ForwardLeft));
    MakeNewActionRule(ActionRuleIds.Line.SlideAttack.Forward,
      new SlideAttackMoveRule(RelativePieceDirection.Forward));
    MakeNewActionRule(ActionRuleIds.Line.SlideAttack.ForwardRight,
      new SlideAttackMoveRule(RelativePieceDirection.ForwardRight));

    MakeNewActionRule(ActionRuleIds.Line.SlideAttack.Left,
      new SlideAttackMoveRule(RelativePieceDirection.Left));
    MakeNewActionRule(ActionRuleIds.Line.SlideAttack.Right,
      new SlideAttackMoveRule(RelativePieceDirection.Right));

    MakeNewActionRule(ActionRuleIds.Line.SlideAttack.BackwardLeft,
      new SlideAttackMoveRule(RelativePieceDirection.BackwardLeft));
    MakeNewActionRule(ActionRuleIds.Line.SlideAttack.Backward,
      new SlideAttackMoveRule(RelativePieceDirection.Backward));
    MakeNewActionRule(ActionRuleIds.Line.SlideAttack.BackwardRight,
      new SlideAttackMoveRule(RelativePieceDirection.BackwardRight));


    // Generic
    MakeNewActionRule(ActionRuleIds.Nothing, new NothingMoveRule());


    // Piece Specific
    MakeNewActionRule(ActionRuleIds.PawnMove, new PawnMoveRule());
    MakeNewActionRule(ActionRuleIds.KnightMove, new KnightMoveRule());
    MakeNewActionRule(ActionRuleIds.Castle, new CastleRule());

    // Piece Specific - Custom
    MakeNewActionRule(ActionRuleIds.WarpBishopMove, new WarpBishopMoveRule());
    MakeNewActionRule(ActionRuleIds.PrawnMove, new LineMoveAttackRule(new PrawnDirectionChooser()));
  }

  internal void InitActionFactories() {
    // Make sure registry is cleared
    ActionFactoriesRegistry.Clear();

    // Register factories for the actions
    RegisterNewAction<MoveAction>(ActionIds.Move);
    RegisterNewAction<AttackAction>(ActionIds.Attack);
    RegisterNewAction<MoveOther>(ActionIds.MoveOther);
    RegisterNewAction<PawnMoveAction>(ActionIds.PawnMove);

    RegisterNewAction<NothingAction>(ActionIds.Nothing);
  }

  internal void InitPieceInfo() {
    // Make sure registry is cleared
    PieceInfoRegistry.Clear();

    // Register Piece Info
    PieceInfo pawnInfo = MakeNewPieceInfo("pawn", "Pawn", "pawn.png");
    pawnInfo.AddActionRule(GetActionRule(ActionRuleIds.Line.Move.Forward), 1);
    pawnInfo.AddActionRule(GetActionRule(ActionRuleIds.Line.SlideAttack.ForwardLeft), 1);
    pawnInfo.AddActionRule(GetActionRule(ActionRuleIds.Line.SlideAttack.ForwardRight), 1);
    pawnInfo.AddActionRule(GetActionRule(ActionRuleIds.PawnMove));

    PieceInfo rookInfo = MakeNewPieceInfo("rook", "Rook", "rook.png");
    rookInfo.AddActionRule(GetActionRule(ActionRuleIds.Line.MoveAttack.Left), 7)
      .AddActionRule(GetActionRule(ActionRuleIds.Line.MoveAttack.Forward), 7);
    rookInfo.AddActionRule(GetActionRule(ActionRuleIds.Line.MoveAttack.Right), 7)
      .AddActionRule(GetActionRule(ActionRuleIds.Line.MoveAttack.Backward), 7);
    MakeNewPieceInfo("knight", "Knight", "knight.png")
      .AddActionRule(GetActionRule(ActionRuleIds.KnightMove), 3);

    PieceInfo bishopInfo = MakeNewPieceInfo("bishop", "Bishop", "bishop.png");
    bishopInfo.AddActionRule(GetActionRule(ActionRuleIds.Line.MoveAttack.ForwardLeft), 7)
      .AddActionRule(GetActionRule(ActionRuleIds.Line.MoveAttack.ForwardRight), 7);
    bishopInfo.AddActionRule(GetActionRule(ActionRuleIds.Line.MoveAttack.BackwardLeft), 7)
      .AddActionRule(GetActionRule(ActionRuleIds.Line.MoveAttack.BackwardRight), 7);

    PieceInfo queenInfo = MakeNewPieceInfo("queen", "Queen", "queen.png");
    queenInfo.AddActionRule(GetActionRule(ActionRuleIds.Line.MoveAttack.Left), 7)
      .AddActionRule(GetActionRule(ActionRuleIds.Line.MoveAttack.Forward), 7);
    queenInfo.AddActionRule(GetActionRule(ActionRuleIds.Line.MoveAttack.Right), 7)
      .AddActionRule(GetActionRule(ActionRuleIds.Line.MoveAttack.Backward), 7);
    queenInfo.AddActionRule(GetActionRule(ActionRuleIds.Line.MoveAttack.ForwardLeft), 7)
      .AddActionRule(GetActionRule(ActionRuleIds.Line.MoveAttack.ForwardRight), 7);
    queenInfo.AddActionRule(GetActionRule(ActionRuleIds.Line.MoveAttack.BackwardLeft), 7)
      .AddActionRule(GetActionRule(ActionRuleIds.Line.MoveAttack.BackwardRight), 7);

    PieceInfo kingInfo = MakeNewPieceInfo("king", "King", "king.png");
    kingInfo.AddActionRule(GetActionRule(ActionRuleIds.Line.MoveAttack.Left), 1)
      .AddActionRule(GetActionRule(ActionRuleIds.Line.MoveAttack.Right), 1);
    kingInfo.AddActionRule(GetActionRule(ActionRuleIds.Line.MoveAttack.Forward), 1)
      .AddActionRule(GetActionRule(ActionRuleIds.Line.MoveAttack.Backward), 1);
    kingInfo.AddActionRule(GetActionRule(ActionRuleIds.Line.MoveAttack.ForwardLeft), 1, true)
      .AddActionRule(GetActionRule(ActionRuleIds.Line.MoveAttack.ForwardRight), 1, true);
    kingInfo.AddActionRule(GetActionRule(ActionRuleIds.Line.MoveAttack.BackwardLeft), 1, true)
      .AddActionRule(GetActionRule(ActionRuleIds.Line.MoveAttack.BackwardRight), 1, true);
    kingInfo.AddActionRule(GetActionRule(ActionRuleIds.Castle));

    // Register custom Piece Info
    PieceInfo warpBishopInfo = MakeNewPieceInfo("warp_bishop", "Warp Bishop", "warp_bishop.png");
    warpBishopInfo.AddActionRule(GetActionRule(ActionRuleIds.WarpBishopMove), 7);
    warpBishopInfo.AddActionRule(GetActionRule(ActionRuleIds.Line.SlideAttack.ForwardLeft), 2,
      true);
    warpBishopInfo.AddActionRule(GetActionRule(ActionRuleIds.Line.SlideAttack.ForwardRight), 2,
      true);
    warpBishopInfo.AddActionRule(GetActionRule(ActionRuleIds.Line.SlideAttack.BackwardLeft), 2,
      true);
    warpBishopInfo.AddActionRule(GetActionRule(ActionRuleIds.Line.SlideAttack.BackwardRight), 2,
      true);

    MakeNewPieceInfo("rock", "Rock", "rock.png")
      .AddActionRule(GetActionRule(ActionRuleIds.Nothing));
    MakeNewPieceInfo("prawn", "Prawn", "prawn.png")
      .AddActionRule(GetActionRule(ActionRuleIds.PrawnMove), 2);

    PieceInfo blockadeInfo = MakeNewPieceInfo("blockade", "Blockade", "blockade.png");
    blockadeInfo.Tags.Add(InvinciblePieceRule.InvincibleTag);
    blockadeInfo.AddActionRule(GetActionRule(ActionRuleIds.Line.Move.Left), 2);
    blockadeInfo.AddActionRule(GetActionRule(ActionRuleIds.Line.Move.Right), 2);
  }

  internal void InitCardFactories() {
    // Make sure registry is cleared
    CardFactoryRegistry.Clear();

    // Register Card Factories
    AddNewCardFactory(CardIds.Shapeshift, new SimpleCardFactory<ShapeshiftCard>());
    AddNewCardFactory(CardIds.PawnArmy, new SimpleCardFactory<PawnArmyCard>());
    AddNewCardFactory(CardIds.Shuffle, new SimpleCardFactory<ShuffleCard>());
    AddNewCardFactory(CardIds.LonelyPieces, new SimpleCardFactory<LonelyPiecesStuckCard>(), true);
    AddNewCardFactory(CardIds.FriendlyFire, new SimpleCardFactory<FriendlyFireCard>(), true);
    AddNewCardFactory(CardIds.BiggerBoard, new SimpleCardFactory<BiggerBoardCard>());
    AddNewCardFactory(CardIds.LevelUp, new SimpleCardFactory<LevelUpCard>());

    // Minor Cards
    // Rules
    AddNewCardFactory(CardIds.Promotion, new PomotionCardFactory());
    AddNewCardFactory(CardIds.Rules.MoveAttack,
      new RandomPieceRuleCardFactory(
        new[] {
          ActionRuleIds.Line.MoveAttack.Forward,
          ActionRuleIds.Line.MoveAttack.Left,
          ActionRuleIds.Line.MoveAttack.Right,
          ActionRuleIds.Line.MoveAttack.Backward
        },
        "Move Further", "move and attack one further horizontally and vertically, up to 3 spaces.",
        3
      ), true, true, true
    );
    AddNewCardFactory(CardIds.Rules.MoveAttackDiagonal,
      new RandomPieceRuleCardFactory(
        new[] {
          ActionRuleIds.Line.MoveAttack.ForwardLeft,
          ActionRuleIds.Line.MoveAttack.ForwardRight,
          ActionRuleIds.Line.MoveAttack.BackwardLeft,
          ActionRuleIds.Line.MoveAttack.BackwardRight
        },
        "Diag. Move Further", "move and attack one further diagonally, up to 3 spaces.", 3
      ), true, true, true
    );
    AddNewCardFactory(CardIds.Rules.Nothing,
      new RandomPieceRuleCardFactory(ActionRuleIds.Nothing, "Nothing", "do nothing.", 1));

    // Piece
    ChangePieceFactory =
      AddNewCardFactory(CardIds.ChangePiece, new ChangePieceCardFactory(), true, false);

    // Space
  }

  internal void InitCardDecks(bool isServer) {
    // Dispose
    if (MajorCardDeck != null) {
      if (IsInstanceValid(MajorCardDeck)) {
        MajorCardDeck.Free();
      }

      MajorCardDeck = null;
    }

    if (MinorCardDeck != null) {
      if (IsInstanceValid(MinorCardDeck)) {
        MinorCardDeck.Free();
      }

      MinorCardDeck = null;
    }

    // If not the server, no need to create the card decks
    if (!isServer) {
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
    MinorCardDeck.AddCard(GetCardFactory(CardIds.Rules.MoveAttack), CardWeights.Common);
    MinorCardDeck.AddCard(GetCardFactory(CardIds.Rules.MoveAttackDiagonal), CardWeights.Common);
    MinorCardDeck.AddCard(GetCardFactory(CardIds.Rules.Nothing), CardWeights.Uncommon);
    AddChild(MinorCardDeck);
  }

  internal void InitInitialCards() {
    // TODO: Make this server-only, and send all initial cards when the game starts.
    var newCard = (PromotionCard)CardFactoryRegistry.GetValue("minor_promotion")
      .CreateNewBlankCard(CurrentGameState);
    newCard.FromPiece = "pawn";
    newCard.ToPiece = new Array<string> { "knight", "bishop", "rook", "queen" };
    AddCard(newCard);
  }


  private void MakeNewValidationRule(string id, ValidationRuleBase newRule,
    bool makeInitialRule = false) {
    newRule.RuleId = id;
    ValidationRuleRegistry.Register(id, newRule);
    if (makeInitialRule) {
      InitialValidationRules.Add(id);
    }

    GD.Print($"Made new Validation Rule: {id}");
  }

  public bool TryGetValidationRule(string id, out ValidationRuleBase rule) {
    return ValidationRuleRegistry.TryGetValue(id, out rule);
  }

  public ValidationRuleBase GetValidationRule(string id) {
    return ValidationRuleRegistry.GetValue(id);
  }


  private void MakeNewActionRule(string id, ActionRuleBase newRule, int defaultLevel = 1) {
    newRule.RuleId = id;
    newRule.DefaultLevel = defaultLevel;
    ActionRuleRegistry.Register(id, newRule);
    GD.Print($"Made new Action Rule: {id}");
  }

  public bool TryGetActionRule(string id, out ActionRuleBase rule) {
    return ActionRuleRegistry.TryGetValue(id, out rule);
  }

  public ActionRuleBase GetActionRule(string id) {
    return ActionRuleRegistry.GetValue(id);
  }


  private ActionFactory RegisterNewAction<T>(string id) where T : ActionBase, new() {
    // Update the Action's actionId
    var newFactory = new SimpleActionFactory<T>();
    newFactory.ActionId = id;
    ActionFactoriesRegistry.Register(id, newFactory);
    GD.Print($"Made new Action Factory for {typeof(T).Name}: {id}");
    return newFactory;
  }

  private PieceInfo MakeNewPieceInfo(string id, string displayName = null,
    string textureLocation = "default.png",
    int defaultLevel = 0) {
    var newInfo = new PieceInfo(id, displayName, defaultLevel);

    foreach (string ruleId in InitialValidationRules) {
      newInfo.AddValidationRule(ValidationRuleRegistry.GetValue(ruleId));
    }

    newInfo.TextureLoc = textureLocation;
    PieceInfoRegistry.Register(id, newInfo);
    GD.Print(
      $"Made new Piece Info: {id} (defaultLevel: {defaultLevel}, textureLocation: {textureLocation})");
    return newInfo;
  }

  private CardFactory AddNewCardFactory(string id, CardFactory newFactory, bool serverOnly = false,
    bool displayCard = true, bool immediateUse = false) {
    newFactory.CardId = id;
    newFactory.ServerOnly = serverOnly;
    newFactory.ImmediateUse = immediateUse;
    newFactory.DisplayCard = displayCard;
    CardFactoryRegistry.Register(id, newFactory);
    return newFactory;
  }

  public CardFactory GetCardFactory(string id) {
    return CardFactoryRegistry.GetValue(id);
  }

  public bool TryGetCardFactory(string id, out CardFactory factory) {
    return CardFactoryRegistry.TryGetValue(id, out factory);
  }
}