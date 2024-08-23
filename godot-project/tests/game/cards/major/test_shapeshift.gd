extends GameTest

var card_script: CSharpScript = preload("res://scripts/game/card/major/ShapeshiftCard.cs")

func test_card() -> void:
	
	var card = card_script.new()
	
	# Create a simple board of pieces
	#  . 🐴🤴.
	# ♟️  .   . ♗
	var pawn = game_state.PlacePiece("pawn", 0, 0, 0, 0, -1)
	var bishop = game_state.PlacePiece("bishop", 0, 0, 3, 0, -1)
	var e_knight = game_state.PlacePiece("knight", 0, 1, 1, 1, -1)
	var e_queen = game_state.PlacePiece("queen", 0, 1, 2, 1, -1)
	
	start_game()
	
	# Add the card
	game_controller.AddCard(card)
	
	# Nothing should have changed
	assert_eq(pawn.info.pieceId, "pawn", "Piece should still be a pawn.")
	assert_eq(e_knight.info.pieceId, "knight", "Piece should still be a knight.")
	assert_eq(e_queen.info.pieceId, "queen", "Piece should still be a queen.")
	
	# First, the pawn should take the knight
	game_state.TakeActionAt(Vector2i(1,1), pawn)
	
	# The pawn should now be a knight
	assert_eq(pawn.info.pieceId, "knight", "Pawn should have changed into a knight.")
	
	# On the queen's turn, take the pawn that just changed into a knight
	next_turn()
	
	game_state.TakeActionAt(Vector2i(1,1), e_queen)
	
	# The queen should now be a knight
	assert_eq(e_queen.info.pieceId, "knight", "Queen should have changed into a knight.")
	
	# Skip back to player 2's turn
	next_turn()
	next_turn()
	
	if not piece_has_actions_at(e_queen, Vector2i(3,0)):
		fail_test("Queen should have actions at (3,0) from (1,1), as it should be a knight.")
	
	# Player 2 should be able to take the bishop with the knight
	game_state.TakeActionAt(Vector2i(3,0), e_queen)
	
	# The queen should now be a bishop
	assert_eq(e_queen.info.pieceId, "bishop", "Queen->Knight should have changed into a bishop.")
