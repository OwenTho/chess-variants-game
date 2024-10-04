extends GameTest

var card_script: CSharpScript = preload("res://scripts/game/card/major/BiggerBoardCard.cs")

func test_card() -> void:
	
	var card = card_script.new()
	card.CardId = "bigger_board"
	
	# Place 2 pieces on the board, at the bottom left corner
	var bottom_corner: Vector2i = game_controller.GridLowerCorner
	var queen = place_piece("queen", 0, 0, 0, 0)
	var e_rook = place_piece("rook", 0, 1, 0, 1)
	
	start_game()
	
	# Neither piece should be able to move off the board
	assert_false(piece_act_at(queen, -1, 0), "Queen should not be able to act off the board (-1, 0).")
	assert_false(piece_act_at(queen, -2, 0), "Queen should not be able to act off the board (-2, 0).")
	assert_false(piece_act_at(queen, -1, -1), "Queen should not be able to act off the board (-1, -1).")
	assert_false(piece_act_at(queen, -2, -2), "Queen should not be able to act off the board (-2, -2).")
	move_piece(queen, 0, 0) # Move the queen back just in case
	next_turn(1)
	assert_false(piece_act_at(e_rook, -1, 1), "Enemy Rook should not be able to act off the board (-1, 1).")
	assert_false(piece_act_at(e_rook, -2, 1), "Enemy Rook should not be able to act off the board (-2, 1).")
	move_piece(e_rook, 0, 1) # Move the enemy rook back just in case
	
	# Add the card
	add_card(card)
	
	# Swap turn over to update move validation
	next_turn(0)
	
	
	# The pieces should now be able to act off the board
	assert_true(piece_act_at(queen, -1, 0), "Queen should be able to act off the board (-1, 0).")
	assert_true(piece_act_at(queen, -2, 0), "Queen should be able to act off the board (-2, 0).")
	assert_true(piece_act_at(queen, -1, -1), "Queen should be able to act off the board (-1, -1).")
	assert_true(piece_act_at(queen, -2, -2), "Queen should be able to act off the board (-2, -2).")
	move_piece(queen, -2, 0)
	next_turn(1)
	assert_true(piece_act_at(e_rook, -1, 1), "Enemy Rook should be able to act off the board (-1, 1).")
	assert_true(piece_act_at(e_rook, -2, 1), "Enemy Rook should be able to act off the board (-2, 1).")
	move_piece(e_rook, -2, 1)
	
	# Queen should be able to take enemy rook off the board.
	next_turn(0)
	assert_true(piece_act_at(queen, -2, 1), "Queen should be able to act to move to (-2, 1).")
	assert_false(piece_on_board(e_rook), "Enemy Rook should no longer be on the board.")
