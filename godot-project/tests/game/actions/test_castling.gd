extends GutTest

const min_dist: int = 3
const max_dist: int = 6
const start_check: int = 1
const end_check: int = 7

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



# Must be left-most and right-most positions to avoid the "OutsideBoardRule"
func castle_test(king, dir: Vector2i, act_pos: Vector2i) -> void:
	var king_start: Vector2i = king.Cell.Pos
	for i in range(start_check, end_check+1):
		var rook = game.place_piece("rook", 0, 0, king.Cell.X + i * dir.x, king.Cell.Y + i * dir.y)
		# Swap turns twice so it's player 0's turn again (and actions are updated)
		game.next_turn(0)
		
		var should_fail := true
		if i <= max_dist and i >= min_dist:
			should_fail = false
		
		# There should be no action of the king's at the castling location
		var result := game.piece_has_actions_at_pos(king, act_pos)
		if (result and should_fail) or ((not result) and (not should_fail)):
			if should_fail:
				fail_test("[Distance %s] King should not be able to castle." % [i])
			else:
				fail_test("[Distance %s] King should be able to castle." % [i])
		
		if should_fail:
			assert_false(game.piece_act_at_pos(king, act_pos), "[Distance %s] King should not have been able to act." % i)
		else:
			assert_true(game.piece_act_at_pos(king, act_pos), "[Distance %s] King should have castled." % i)
			# Check that the Rook has moved
			assert_true(game.piece_on_cell(rook, (act_pos.x - dir.x), act_pos.y), "[Distance %s] Rook should not have moved." % i)
		# Remove the rook before continuing
		game.take_piece(rook)
		# Move the King back
		game.move_piece(king, king_start.x, king_start.y)
		king.TimesMoved = 0


func test_distance_right() -> void:
	
	game.start_game()
	# Place King
	# 👑. . . . . . .
	var king = game.place_piece("king", 0, 0, 0, 0)
	
	castle_test(king, Vector2i(1,0), Vector2i(2,0))


func test_distance_left() -> void:
	
	game.start_game()
	# Place King
	# 👑. . . . . . .
	var king = game.place_piece("king", 0, 0, 7, 0)
	
	castle_test(king, Vector2i(-1,0), Vector2i(5,0))




func test_castle_blocked() -> void:
	
	# Place Pieces
	# 👑♟️ . 🏰
	var king = game.place_piece("king", 0, 0, 0, 0)
	var pawn = game.place_piece("pawn", 0, 0, 1, 0)
	var rook = game.place_piece("rook", 0, 0, 3, 0)
	
	game.start_game()
	
	# King should have the action to castle
	assert_true(game.piece_has_actions_at(king, 2, 0), "King should have castling actions.")
	
	# King should not be able to castle, however
	assert_false(game.piece_act_at(king, 2, 0), "King should not have been able to castle with pawn in the way.")
	
	# Move pawn forwards
	game.move_piece(pawn, 1, 1)
	
	# Skip to next turn
	game.next_turn()
	game.next_turn()
	
	# King should still have the action to castle
	assert_true(game.piece_has_actions_at(king, 2, 0), "King should have castling actions.")
	
	# King should be able to castle
	assert_true(game.piece_act_at(king, 2, 0), "King should have been able to castle with the pawn out of the way.")
	# Rook should also have moved
	assert_true(game.piece_on_cell(rook, 1, 0), "Rook should have moved.")




func test_cant_castle_after_move() -> void:
	
	# Place Pieces
	# 👑. . . 🏰
	var king = game.place_piece("king", 0, 0, 0, 0)
	var rook = game.place_piece("rook", 0, 0, 4, 0)
	
	game.start_game()
	
	# King should have the action to castle
	assert_true(game.piece_has_actions_at(king, 2, 0), "King should have castling actions.")
	
	# Move King right one
	# . 👑. . 🏰
	assert_true(game.piece_act_at(king, 1, 0), "King should be able to move right.")
	assert_true(game.piece_on_cell(rook, 4, 0), "Rook should not have moved.")
	
	# Skip to next turn
	game.next_turn()
	game.next_turn()
	
	# King should not have the action to castle
	assert_false(game.piece_has_actions_at(king, 3, 0), "King should not have castling actions.")
	
	# King should not be able to castle
	assert_false(game.piece_act_at(king, 3, 0), "King should not be able to castle.")
	# Rook should not have moved
	assert_true(game.piece_on_cell(rook, 4, 0), "Rook should not have moved.")




func test_enemy_rook() -> void:
	
	# Place Pieces
	# 👑. . 🏰
	var king = game.place_piece("king", 0, 0, 0, 0)
	var rook = game.place_piece("rook", 0, 1, 3, 0)
	
	game.start_game()
	
	# Check if the King can castle
	assert_false(game.piece_has_actions_at(king, 2, 0), "King can castle with enemy rook.")
	
	# Test castle
	assert_false(game.piece_act_at(king, 2, 0), "King was able to castle, when it shouldn't be able to.")
	
	# Check that rook is in the same place
	assert_true(game.piece_on_cell(rook, 3, 0), "Rook moved, when it shouldn't have been moved.")
