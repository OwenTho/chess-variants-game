extends GameTest


func test_protected() -> void:
	
	# Place Pieces
	# 👑. ♟️🏰
	var king = place_piece("king", 0, 0, 0, 0)
	place_piece("pawn", 0, 0, 2, 0)
	place_piece("rook", 0, 1, 3, 0)
	
	start_game()
	
	# King should not be in check
	assert_false(game_state.PlayerInCheck(0))




func test_protected_by_king() -> void:
	
	# Place Pieces
	# . 👑. 👑🏰
	var king = place_piece("king", 0, 0, 1, 0)
	place_piece("king", 0, 1, 3, 0)
	place_piece("rook", 0, 1, 4, 0)
	
	start_game()
	
	# King should not be in check
	assert_false(game_state.PlayerInCheck(0))
	
	# King should not be able to move right because it would be check
	assert_false(game_state.TakeActionAt(Vector2i(2,0), king))
	
	# King should be able to move left
	# 👑. . 👑🏰
	assert_true(game_state.TakeActionAt(Vector2i(0,0), king))
	
	next_turn()
	
	# King should still not be in check
	assert_false(game_state.PlayerInCheck(0))




func test_attack() -> void:
	
	# Place Pieces
	# 👑. . 🏰
	var king = place_piece("king", 0, 0, 0, 0)
	place_piece("rook", 0, 1, 3, 0)
	
	start_game()
	
	# King should be in check
	assert_true(game_state.PlayerInCheck(0))
	
	# King shouldn't be able to move closer
	assert_false(game_state.TakeActionAt(Vector2i(1, 0), king))




func test_attack_miss() -> void:
	
	# Place Pieces
	# . . . 🏰
	# 👑. . . 
	var king = place_piece("king", 0, 0, 0, 0)
	place_piece("rook", 0, 1, 3, 1)
	
	start_game()
	
	# King should not be in check
	assert_false(game_state.PlayerInCheck(0))
	
	# Moving up shouldn't work
	assert_false(game_state.TakeActionAt(Vector2i(0,1), king))
	
	# King should be able to move right
	# . . . 🏰
	# . 👑. . 
	assert_true(game_state.TakeActionAt(Vector2i(1,0), king))
	
	next_turn()
	
	# King should still not be in check
	assert_false(game_state.PlayerInCheck(0))
