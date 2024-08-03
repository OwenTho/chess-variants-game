extends GameTest


func test_move_to_check() -> void:
	
	# Place pieces
	# 👑.  
	# . 🏰
	var king = game_state.PlacePiece("king", 0, 0, 0, 1, -1)
	var rook = game_state.PlacePiece("rook", 1, 1, 1, 0, -1)
	
	game_controller.StartGame()
	
	# King should not be in check
	assert_false(game_state.PlayerInCheck(0))
	
	# King shouldn't be able to move in to check
	assert_false(game_state.TakeActionAt(Vector2i(0,0), king))




func test_attack_defended_piece() -> void:
	# Place pieces
	# 👑. . 
	# . 🏰🏰
	var king = game_state.PlacePiece("king", 0, 0, 0, 1, -1)
	var e_rook1 = game_state.PlacePiece("rook", 1, 1, 1, 0, -1)
	var e_rook2 = game_state.PlacePiece("rook", 2, 1, 2, 0, -1)
	
	game_controller.StartGame()
	
	# King should not be in check
	assert_false(game_state.PlayerInCheck(0))
	
	# King shouldn't be able to take the rook
	assert_false(game_state.TakeActionAt(Vector2i(0,1), king))




func test_move_defending_piece() -> void:
	
	# Place pieces
	# . . .
	# 👑♟️🏰 
	var king = game_state.PlacePiece("king", 0, 0, 0, 0, -1)
	var pawn = game_state.PlacePiece("pawn", 1, 0, 1, 0, -1)
	var e_rook = game_state.PlacePiece("rook", 2, 1, 3, 0, -1)
	
	game_controller.StartGame()
	
	# King should not be in check
	assert_false(game_state.PlayerInCheck(0))
	
	# Try to move pawn up
	# . ♟️ . .
	# 👑. . 🏰 

	assert_false(game_state.TakeActionAt(Vector2i(1,1), pawn))




func test_move_non_save() -> void:
	# Place pieces
	# . ♟️ . .
	# 👑. . 🏰 
	var king = game_state.PlacePiece("king", 0, 0, 0, 0, -1)
	var pawn = game_state.PlacePiece("pawn", 1, 0, 1, 1, -1)
	var e_rook = game_state.PlacePiece("rook", 2, 1, 3, 0, -1)
	
	game_controller.StartGame()
	
	# King should be in check
	assert_true(game_state.PlayerInCheck(0))
	
	# Try to move pawn up, when king is in check
	# . ♟️ . .
	# . . . .
	# 👑. . 🏰 

	assert_false(game_state.TakeActionAt(Vector2i(1,2), pawn))
