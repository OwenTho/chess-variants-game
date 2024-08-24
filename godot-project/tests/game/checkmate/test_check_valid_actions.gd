extends GameTest

func test_teammates_dont_check() -> void:
	
	# Place pieces
	# 👑. 🏰.
	var king = place_piece("king", 0, 0, 0, 0)
	var rook = place_piece("rook", 0, 0, 2, 0)
	
	start_game()
	
	# King shouldn't be in check
	assert_false(game_state.PlayerInCheck(0), "King should not start in check.")
	
	# Move Rook to the side
	# 👑. . 🏰
	# If it was check, this would be an invalid move
	assert_true(piece_act_at(rook, 3, 0), "Rook should be able to move to the side, as it should not put the King in check.")
	next_turn(1)
	
	# King still shouldn't be in check
	assert_false(game_state.PlayerInCheck(0), "King should still not be in check.")
	
	next_turn(0)
	
	# Move king to the side
	# . 👑. 🏰
	# If it was check, this would be an invalid move
	assert_true(piece_act_at(king, 1, 0), "King should be able to move towards the Rook.")
	
	next_turn(1)
	
	# King still shouldn't be in check
	assert_false(game_state.PlayerInCheck(0), "King should still not be in check.")




func test_move_when_protected() -> void:
	
	# Place Pieces
	# 👑. ♟️🏰
	var king = place_piece("king", 0, 0, 0, 0)
	place_piece("pawn", 0, 0, 2, 0)
	place_piece("rook", 0, 1, 3, 0)
	
	start_game()
	
	# King should not be in check
	assert_false(game_state.PlayerInCheck(0))
	
	# King should be able to move right
	# . 👑♟️🏰
	assert_true(piece_act_at(king, 1, 0))
	
	next_turn()
	
	# King should still not be in check
	assert_false(game_state.PlayerInCheck(0))




func test_move_when_protected_by_enemy() -> void:
	
	# Place Pieces
	# 👑. ♟️🏰
	var king = place_piece("king", 0, 0, 0, 0)
	place_piece("pawn", 0, 1, 2, 0)
	place_piece("rook", 0, 1, 3, 0)
	
	start_game()
	
	# King should not be in check
	assert_false(game_state.PlayerInCheck(0))
	
	# King should be able to move right
	# . 👑♟️🏰
	assert_true(piece_act_at(king, 1, 0))
	
	next_turn()
	
	# King should still not be in check
	assert_false(game_state.PlayerInCheck(0))




func test_move_from_check() -> void:
	
	# Place pieces
	# . .
	# 👑🏰
	var king = place_piece("king", 0, 0, 0, 0)
	var rook = place_piece("rook", 0, 1, 1, 0)
	
	start_game()
	
	# King should be in check
	assert_true(game_state.PlayerInCheck(0))
	
	# Move King up one
	# 👑.
	# . 🏰
	# Move should succeed 
	assert_true(piece_act_at(king, 0, 1))
	next_turn()
	
	# King shouldn't be in check
	assert_false(game_state.PlayerInCheck(0))




func test_can_take_attacker() -> void:
	
	# Place Pieces
	# . 👑🏰
	var king = place_piece("king", 0, 0, 1, 0)
	var rook = place_piece("rook", 0, 1, 2, 0)
	
	start_game()
	
	# King should be in check
	assert_true(game_state.PlayerInCheck(0), "King should be in check.")
	
	# Moving left should check
	assert_true(game_state.DoesActionCheck(Vector2i(0, 0), king), "King should be in check if they move left.")
	
	# King should be able to take the rook
	assert_true(piece_act_at(king, 2, 0), "King should be able to take the rook.")
	
	# Rook should have been taken
	assert_false(piece_on_board(rook), "The Rook should no longer be on the board.")
	
	next_turn()
	
	# King should no longer be in check
	assert_false(game_state.PlayerInCheck(0), "King should no longer be in check.")




func test_enemy_can_check() -> void:
	
	# Place Pieces
	# 👑. . . 
	# . . . 🏰
	place_piece("king", 0, 0, 0, 1)
	var rook = place_piece("rook", 0, 1, 3, 0)
	
	start_game()
	
	# King is not in check
	assert_false(game_state.PlayerInCheck(0), "King should not be in check.")
	
	# Skip to enemy turn
	next_turn(1)
	
	# Enemy can move rook to check king
	# 👑. . 🏰
	# . . . .
	assert_true(piece_act_at(rook, 3, 1), "Rook should be able to move to check King")
	
	next_turn(0)
	
	# King should be in check
	assert_true(game_state.PlayerInCheck(0), "King should be in check after Rook is moved.")




func test_enemy_can_check_indirect() -> void:
	
	# Place Pieces
	# . . . .
	# 👑. 🐴🏰
	place_piece("king", 0, 0, 0, 0)
	var knight = place_piece("knight", 0, 1, 2, 0)
	place_piece("rook", 0, 1, 3, 0)
	
	start_game()
	
	# King is not in check
	assert_false(game_state.PlayerInCheck(0), "King should not be in check.")
	
	# Skip to enemy turn
	next_turn(1)
	
	# Enemy can move knight out of the way to check king
	# 🐴. . .
	# 👑. . 🏰
	assert_true(piece_act_at(knight, 0, 1), "Enemy should be able to move Knight out of the way of the Rook.")
	
	next_turn(0)
	
	# King should be in check
	assert_true(game_state.PlayerInCheck(0), "King should be in check.")
