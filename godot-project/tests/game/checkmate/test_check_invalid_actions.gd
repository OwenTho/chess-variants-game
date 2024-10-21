extends GutTest

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



func test_move_to_check() -> void:
	
	# Place pieces
	# 👑.  
	# . 🏰
	var king = game.place_piece("king", 0, 0, 0, 1)
	var rook = game.place_piece("rook", 1, 1, 1, 0)
	
	game.start_game()
	
	# King should not be in check
	assert_false(game.game_state.PlayerInCheck(0))
	
	# King shouldn't be able to move in to check
	assert_false(game.piece_act_at(king, 0, 0), "King should not be able to move into check.")




func test_attack_defended_piece() -> void:
	# Place pieces
	# 👑. . 
	# . 🏰🏰
	var king = game.place_piece("king", 0, 0, 0, 1)
	var e_rook1 = game.place_piece("rook", 1, 1, 1, 0)
	var e_rook2 = game.place_piece("rook", 2, 1, 2, 0)
	
	game.start_game()
	
	# King should not be in check
	assert_false(game.game_state.PlayerInCheck(0), "Team 0 should not be in Check.")
	
	# King shouldn't be able to take the rook
	assert_false(game.piece_act_at(king, 0, 1), "King should not be able to take the rook.")




func test_move_defending_piece() -> void:
	
	# Place pieces
	# . . .
	# 👑♟️🏰 
	var king = game.place_piece("king", 0, 0, 0, 0)
	var pawn = game.place_piece("pawn", 1, 0, 1, 0)
	var e_rook = game.place_piece("rook", 2, 1, 3, 0)
	
	game.start_game()
	
	# King should not be in check
	assert_false(game.game_state.PlayerInCheck(0), "King should not be in check.")
	
	# Try to move pawn up
	# . ♟️ . .
	# 👑. . 🏰 

	assert_false(game.piece_act_at(pawn, 1, 1), "Pawn should not be able to move up.")




func test_move_non_save() -> void:
	# Place pieces
	# . ♟️ . .
	# 👑. . 🏰 
	var king = game.place_piece("king", 0, 0, 0, 0)
	var pawn = game.place_piece("pawn", 1, 0, 1, 1)
	var e_rook = game.place_piece("rook", 2, 1, 3, 0)
	
	game.start_game()
	
	# King should be in check
	assert_true(game.game_state.PlayerInCheck(0), "King should not be in check.")
	
	# Try to move pawn up, when king is in check
	# . ♟️ . .
	# . . . .
	# 👑. . 🏰 

	assert_false(game.piece_act_at(pawn, 1, 2), "Pawn should not be able to move forward while the King is in Check.")
