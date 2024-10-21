extends GutTest

var card_scene: CSharpScript = preload("res://scripts/game/card/major/SinglePieceArmyCard.cs")
var game: GameTest = GameTest.new()

func before_all() -> void:
	add_child(game)
	game.before_all()

func before_each() -> void:
	game.before_each()


func after_each() -> void:
	game.after_each()

func after_all() -> void:
	game.after_all()
	game.free()



func test_card() -> void:
	
	var card = card_scene.new()
	card.CardId = "single_piece_army"
	
	# By default, armyPiece should be empty (or null)
	assert_true(card.ArmyPiece == null or card.ArmyPiece.is_empty())
	
	# Add pieces to the board
	# 👑🏰♟️🐴♗
	game.place_piece("king", 0, 0, 0, 0)
	game.place_piece("rook", 0, 0, 1, 0)
	game.place_piece("pawn", 0, 0, 2, 0)
	game.place_piece("knight", 0, 0, 3, 0)
	game.place_piece("bishop", 0, 0, 4, 0)
	
	# As none are queen, set the piece to be "queen"
	card.ArmyPiece = "queen"
	
	# Start game to update piece actions
	game.start_game()
	
	# None of the pieces should be a queen
	for piece in game.game_state.AllPieces:
		if piece == null:
			fail_test("Null piece in allPieces (before card).")
			continue
		if piece.Info == null:
			fail_test("Piece has null info (should be queen) (before card).")
			continue
		assert_false(game.piece_has_piece_id(piece, "queen"), "Piece should not have 'queen' PieceInfo (before card).")
		# Pieces should not need action updates (as turn has not yet passed over)
		assert_false(piece.NeedsActionUpdate, "Piece should not need an action update yet (before card).")
	
	# Upon adding the card, all pieces should now be queens
	# 🤴🤴🤴🤴🤴
	game.add_card(card)
	
	for piece in game.game_state.AllPieces:
		if piece == null:
			fail_test("Null piece in allPieces.")
			continue
		if piece.Info == null:
			fail_test("Piece has null info (should be queen).")
			continue
		assert_true(game.piece_has_piece_id(piece, "queen"), "Piece should have 'queen' PieceInfo.")
		assert_true(piece.NeedsActionUpdate, "Piece should need an action update.")
	
