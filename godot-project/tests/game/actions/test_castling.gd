extends GameTest

const min_dist: int = 3
const max_dist: int = 6
const start_check: int = 1
const end_check: int = 7

# Must be left-most and right-most positions to avoid the "OutsideBoardRule"
func castle_test(king, dir: Vector2i, act_pos: Vector2i) -> void:
	var king_start: Vector2i = king.cell.pos
	for i in range(start_check, end_check+1):
		var rook = game_state.PlacePiece("rook", 0, 0, king.cell.x + i * dir.x, king.cell.y + i * dir.y, -1)
		# Swap turns twice so it's player 0's turn again (and actions are updated)
		king.EnableActionsUpdate()
		game_state.NextTurn()
		game_state.NextTurn()
		
		var should_fail := true
		if i <= max_dist and i >= min_dist:
			should_fail = false
		
		# There should be no action of the king's at the castling location
		var result := piece_has_actions_at(king, act_pos)
		if (result and should_fail) or ((not result) and (not should_fail)):
			if should_fail:
				fail_test("[Distance %s] King should not be able to castle." % [i])
			else:
				fail_test("[Distance %s] King should be able to castle." % [i])
		
		if should_fail:
			assert_false(game_state.TakeActionAt(act_pos, king), "[Distance %s] King should not have been able to act." % i)
		else:
			assert_true(game_state.TakeActionAt(act_pos, king), "[Distance %s] King should have castled." % i)
			# Check that the Rook has moved
			assert_eq(rook.cell.x, (act_pos.x - dir.x), "[Distance %s] Rook should not have moved." % i)
		# Remove the rook before continuing
		game_state.TakePiece(rook.id)
		# Move the King back
		game_state.MovePiece(king, king_start.x, king_start.y)
		king.timesMoved = 0

func test_distance_right() -> void:
	
	game_controller.StartGame()
	# Place King
	# 👑. . . . . . .
	var king = game_state.PlacePiece("king", 0, 0, 0, 0, -1)
	
	castle_test(king, Vector2i(1,0), Vector2i(2,0))


func test_distance_left() -> void:
	
	game_controller.StartGame()
	# Place King
	# 👑. . . . . . .
	var king = game_state.PlacePiece("king", 0, 0, 7, 0, -1)
	
	castle_test(king, Vector2i(-1,0), Vector2i(5,0))




func test_castle_blocked() -> void:
	
	# Place Pieces
	# 👑♟️ . 🏰
	var king = game_state.PlacePiece("king", 0, 0, 0, 0, -1)
	var pawn = game_state.PlacePiece("pawn", 0, 0, 1, 0, -1)
	var rook = game_state.PlacePiece("rook", 0, 0, 3, 0, -1)
	
	game_controller.StartGame()
	
	# King should have the action to castle
	if not piece_has_actions_at(king, Vector2i(2,0)):
		fail_test("King should have castling actions.")
	
	# King should not be able to castle, however
	assert_false(game_state.TakeActionAt(Vector2i(2,0), king), "King should not have been able to castle with pawn in the way.")
	
	# Move pawn forwards
	assert_true(game_state.TakeActionAt(Vector2i(1,1), pawn), "Pawn should be able to move forwards.")
	
	# Skip to next turn
	game_state.NextTurn()
	game_state.NextTurn()
	
	# King should still have the action to castle
	if not piece_has_actions_at(king, Vector2i(2,0)):
		fail_test("King should have castling actions.")
	
	# King should be able to castle
	assert_true(game_state.TakeActionAt(Vector2i(2,0), king), "King should have been able to castle with the pawn out of the way.")
	# Rook should also have moved
	assert_eq(rook.cell.pos, Vector2i(1,0), "Rook should have moved.")




func test_cant_castle_after_move() -> void:
	
	# Place Pieces
	# 👑. . . 🏰
	var king = game_state.PlacePiece("king", 0, 0, 0, 0, -1)
	var rook = game_state.PlacePiece("rook", 0, 0, 4, 0, -1)
	
	game_controller.StartGame()
	
	# King should have the action to castle
	if not piece_has_actions_at(king, Vector2i(2,0)):
		fail_test("King should have castling actions.")
	
	# Move King right one
	# . 👑. . 🏰
	assert_true(game_state.TakeActionAt(Vector2i(1,0), king), "King should be able to move right.")
	assert_eq(rook.cell.pos, Vector2i(4,0), "Rook should not have moved.")
	
	# Skip to next turn
	game_state.NextTurn()
	game_state.NextTurn()
	
	# King should not have the action to castle
	if piece_has_actions_at(king, Vector2i(3,0)):
		fail_test("King should not have castling actions.")
	
	# King should not be able to castle
	assert_false(game_state.TakeActionAt(Vector2i(3,0), king), "King should not be able to castle.")
	# Rook should not have moved
	assert_eq(rook.cell.pos, Vector2i(4,0), "Rook should not have moved.")




func test_enemy_rook() -> void:
	
	# Place Pieces
	# 👑. . 🏰
	var king = game_state.PlacePiece("king", 0, 0, 0, 0, -1)
	var rook = game_state.PlacePiece("rook", 0, 1, 3, 0, -1)
	
	game_controller.StartGame()
	
	# Check if the King can castle
	if piece_has_actions_at(king, Vector2i(2,0)):
		fail_test("King can castle with enemy rook.")
	
	# Test castle
	assert_false(game_state.TakeActionAt(Vector2i(2,0), king), "King was able to castle, when it shouldn't be able to.")
	
	# Check that rook is in the same place
	assert_eq(rook.cell.pos, Vector2i(3,0), "Rook moved, when it shouldn't have been moved.")
