extends GameTest

var card_script: CSharpScript = preload("res://scripts/game/card/major/FriendlyFireCard.cs")

func test_card() -> void:
	
	var card = card_script.new()
	
	# Make the board
	#  ♟️  🤴
	var pawn = place_piece("pawn", 0, 0, 0, 0)
	var queen = place_piece("queen", 0, 0, 1, 0)
	
	start_game()
	
	# Initially, queen should not be able to take the pawn
	assert_false(piece_act_at(queen, 0, 0), "The Queen should not be able to act on the pawn's space.")
	assert_true(piece_on_cell(queen, 1, 0), "The Queen should not have moved.")
	assert_true(piece_on_board(pawn), "The Pawn should still be on the board")
	
	# Now add the card
	add_card(card)
	
	# Skip to player's next turn
	next_turn(0)
	
	# Queen should now be able to take the pawn
	assert_true(piece_act_at(queen, 0, 0), "The Queen should be able to act on the pawn's space.")
	assert_true(piece_on_cell(queen, 0, 0), "The Queen should have moved to the Pawn's cell.")
	assert_false(piece_on_board(pawn), "The Pawn should no longer be on the board")
