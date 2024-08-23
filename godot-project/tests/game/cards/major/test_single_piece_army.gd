extends GameTest

var card_scene: CSharpScript = preload("res://scripts/game/card/major/SinglePieceArmyCard.cs")

func test_card() -> void:
	
	var card = card_scene.new()
	
	# By default, armyPiece should be empty (or null)
	assert_true(card.armyPiece == null or card.armyPiece.is_empty())
	
	# Add pieces to the board
	# 👑🏰♟️🐴♗
	game_state.PlacePiece("king", 0, 0, 0, 0, -1)
	game_state.PlacePiece("rook", 0, 0, 1, 0, -1)
	game_state.PlacePiece("pawn", 0, 0, 2, 0, -1)
	game_state.PlacePiece("knight", 0, 0, 3, 0, -1)
	game_state.PlacePiece("bishop", 0, 0, 4, 0, -1)
	
	# As none are queen, set the piece to be "queen"
	card.armyPiece = "queen"
	
	# Start game to update piece actions
	start_game()
	
	# None of the pieces should be a queen
	for piece in game_state.allPieces:
		if piece == null:
			fail_test("Null piece in allPieces (before card).")
			continue
		if piece.info == null:
			fail_test("Piece has null info (should be queen) (before card).")
			continue
		assert_ne(piece.info.pieceId, "queen", "Piece should not have 'queen' PieceInfo (before card).")
		# Pieces should not need action updates (as turn has not yet passed over)
		assert_false(piece.needsActionUpdate, "Piece should not need an action update yet (before card).")
	
	# Upon adding the card, all pieces should now be queens
	# 🤴🤴🤴🤴🤴
	game_controller.AddCard(card)
	
	for piece in game_state.allPieces:
		if piece == null:
			fail_test("Null piece in allPieces.")
			continue
		if piece.info == null:
			fail_test("Piece has null info (should be queen).")
			continue
		assert_eq(piece.info.pieceId, "queen", "Piece should have 'queen' PieceInfo.")
		assert_true(piece.needsActionUpdate, "Piece should need an action update.")
	
