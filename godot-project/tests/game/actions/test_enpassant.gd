extends GameTest


func test_tag() -> void:
	
	# Initialise board
	# . 
	# . 
	# ♟️
	var pawn = game_state.PlacePiece("pawn", 0, 0, 0, 0, -1)
	
	# Start the game
	game_controller.StartGame()
	
	# Do 2 space move on second pawn
	# ♟️ 
	# . 
	# .
	assert_true(game_state.TakeActionAt(Vector2i(0,2), pawn))
	
	# Pawn should have "pawn_initial"
	assert_true(pawn.HasTag("pawn_initial"))
	game_state.NextTurn()
	
	# Pawn should still have "pawn_initial" as it's not that player's turn
	assert_true(pawn.HasTag("pawn_initial"))
	
	game_state.NextTurn()
	
	# Pawn should no longer have the "pawn_initial" tag
	
	assert_false(pawn.HasTag("pawn_initial"))
	
	# Pawn should not be able to move two spaces up,
	# but should be able to move 1 space up
	assert_false(game_state.TakeActionAt(Vector2i(0,4), pawn))
	assert_true(game_state.TakeActionAt(Vector2i(0,3), pawn))





func test_teammate() -> void:
	
	# Initialise board
	# .  .
	# .  .
	# ♟️ ♟️
	var pawn1 = game_state.PlacePiece("pawn", 0, 0, 0, 0, -1)
	var pawn2 = game_state.PlacePiece("pawn", 1, 0, 1, 0, -1)
	
	# Start the game
	game_controller.StartGame()
	
	# Do 2 space move on second pawn
	# .  ♟️
	# .  .
	# ♟️ .
	assert_true(game_state.TakeActionAt(Vector2i(1,2), pawn2))
	
	# Only Pawn 2 should have "pawn_initial"
	assert_false(pawn1.HasTag("pawn_initial"))
	assert_true(pawn2.HasTag("pawn_initial"))
	game_state.NextTurn()
	
	# Make sure tag is still there on next turn
	assert_true(pawn2.HasTag("pawn_initial"))

	game_state.NextTurn()
	# Tag should be gone
	assert_false(pawn2.HasTag("pawn_initial"))
	
	# Move Pawn 1 up 2 spaces
	# ♟️ ♟️
	# .  .
	# .  .
	assert_true(game_state.TakeActionAt(Vector2i(0,2), pawn1))
	
	# Only Pawn 1 should have the tag
	assert_true(pawn1.HasTag("pawn_initial"))
	assert_false(pawn2.HasTag("pawn_initial"))
	
	# Move to next turn
	game_state.NextTurn()
	
	# Pawn 1 should not be able to use En Passant
	var actions_at_pos: int = 0
	for cell in game_state.actionGrid.cells:
		if cell.pos == Vector2i(0,1):
			for item in cell.items:
				if item.owner == pawn2:
					actions_at_pos += 1
	
	# Pawn should only be able to Move + Attack
	if actions_at_pos > 2:
		fail_test("Pawn2 is still able to En Passant, when it shouldn't be able to.")





func test_enemy() -> void:
	
	# Initialise board
	# .  ♟️
	# .  .
	# ♟️ .
	var pawn1 = game_state.PlacePiece("pawn", 0, 0, 0, 0, -1)
	var pawn2 = game_state.PlacePiece("pawn", 0, 1, 1, 2, -1)
	
	# Start the game
	game_controller.StartGame()
	
	# Do 2 space move on first pawn
	# ♟️ ♟️
	# .  .
	# .  .
	assert_true(game_state.TakeActionAt(Vector2i(0,2), pawn1))
	
	# Only Pawn 1 should have "pawn_initial"
	assert_true(pawn1.HasTag("pawn_initial"))
	assert_false(pawn2.HasTag("pawn_initial"))
	game_state.NextTurn()
	# Make sure tag is still there on next turn
	assert_true(pawn1.HasTag("pawn_initial"))
	
	# Enemy should be able to use En Passant
	var failed: bool = true
	for cell in game_state.actionGrid.cells:
		if cell.pos == Vector2i(0,1):
			for item in cell.items:
				if item.owner == pawn2:
					failed = false
	
	if failed:
		fail_test("Pawn2 is unable to En Passant")
	
	# Make sure pawn can En Passant
	# .  .
	# ♟️ .
	# .  .
	assert_true(game_state.TakeActionAt(Vector2i(0,1), pawn2))
	# Pawn 1 should be gone, and Pawn 2 still there
	assert_false(game_state.allPieces.has(pawn1))
	assert_true(game_state.allPieces.has(pawn2))
	game_state.NextTurn()
	# Tag should still be there, as pawn1 is no longer processed
	assert_true(pawn1.HasTag("pawn_initial"))
	
	# Action should be gone
	for cell in game_state.actionGrid.cells:
		if cell.pos == Vector2i(0,1):
			for item in cell.items:
				if item.owner == pawn2:
					fail_test("Action on Pawn 2's space, when there should not be one.")





func test_pawn_taken() -> void:
	
	# Skip through checks already done above
	# Initialise board
	# .  ♟️
	# .  ♗
	# ♟️ .
	var pawn1 = game_state.PlacePiece("pawn", 0, 0, 0, 0, -1)
	var pawn2 = game_state.PlacePiece("pawn", 1, 1, 1, 2, -1)
	var bishop2 = game_state.PlacePiece("bishop", 1, 1, 1, 1, -1)
	
	# Start the game
	game_controller.StartGame()
	
	# Do 2 space move on first pawn
	# ♟️ ♟️
	# .  ♗
	# .  .
	assert_true(game_state.TakeActionAt(Vector2i(0,2), pawn1))
	
	game_state.NextTurn()
	
	# Enemy should be able to use En Passant
	var failed := true
	for cell in game_state.actionGrid.cells:
		if cell.pos == Vector2i(0,1):
			for item in cell.items:
				if item.owner == pawn2:
					failed = false
	
	if failed:
		fail_test("Pawn2 is unable to En Passant")
	
	# Pawn 1 is taken by the bishop
	# ♗ ♟️
	# .  .
	# .  .
	assert_true(game_state.TakeActionAt(Vector2i(0,2), bishop2))
	# Pawn 1 should be gone, while Bishop and Pawn 2 are still there
	assert_false(game_state.allPieces.has(pawn1))
	assert_true(game_state.allPieces.has(pawn2))
	assert_true(game_state.allPieces.has(bishop2))
	
	game_state.NextTurn()
	
	# Enemy should not be able to use En Passant
	var actions_at_pos: int = 0
	for cell in game_state.actionGrid.cells:
		if cell.pos == Vector2i(0,1):
			for item in cell.items:
				if item.owner == pawn2:
					actions_at_pos += 1
	
	# Pawn should only be able to Move + Attack
	if actions_at_pos > 2:
		fail_test("Pawn2 is still able to En Passant, when it shouldn't be able to.")
	
	# Also try to act it out
	assert_false(game_state.TakeActionAt(Vector2i(0,1), pawn2))
