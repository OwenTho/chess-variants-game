extends GameTest


func test_move_to_check() -> void:
	
	# Place pieces
	# 👑.  
	# . 🏰
	var king = place_piece("king", 0, 0, 0, 1)
	var rook = place_piece("rook", 1, 1, 1, 0)
	
	start_game()
	
	# King should not be in check
	assert_false(game_state.PlayerInCheck(0))
	
	# King shouldn't be able to move in to check
	assert_false(piece_act_at(king, 0, 0))




func test_attack_defended_piece() -> void:
	# Place pieces
	# 👑. . 
	# . 🏰🏰
	var king = place_piece("king", 0, 0, 0, 1)
	var e_rook1 = place_piece("rook", 1, 1, 1, 0)
	var e_rook2 = place_piece("rook", 2, 1, 2, 0)
	
	start_game()
	
	# King should not be in check
	assert_false(game_state.PlayerInCheck(0))
	
	# King shouldn't be able to take the rook
	assert_false(piece_act_at(king, 0, 1))




func test_move_defending_piece() -> void:
	
	# Place pieces
	# . . .
	# 👑♟️🏰 
	var king = place_piece("king", 0, 0, 0, 0)
	var pawn = place_piece("pawn", 1, 0, 1, 0)
	var e_rook = place_piece("rook", 2, 1, 3, 0)
	
	start_game()
	
	# King should not be in check
	assert_false(game_state.PlayerInCheck(0))
	
	# Try to move pawn up
	# . ♟️ . .
	# 👑. . 🏰 

	assert_false(piece_act_at(pawn, 1, 1))




func test_move_non_save() -> void:
	# Place pieces
	# . ♟️ . .
	# 👑. . 🏰 
	var king = place_piece("king", 0, 0, 0, 0)
	var pawn = place_piece("pawn", 1, 0, 1, 1)
	var e_rook = place_piece("rook", 2, 1, 3, 0)
	
	start_game()
	
	# King should be in check
	assert_true(game_state.PlayerInCheck(0))
	
	# Try to move pawn up, when king is in check
	# . ♟️ . .
	# . . . .
	# 👑. . 🏰 

	assert_false(piece_act_at(pawn, 1, 2))
