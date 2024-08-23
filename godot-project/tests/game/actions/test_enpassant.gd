extends GameTest




func test_teammate() -> void:
	
	# Initialise board
	# .  .
	# .  .
	# ♟️ ♟️
	var pawn = game_state.PlacePiece("pawn", 0, 0, 0, 0, -1)
	var e_pawn = game_state.PlacePiece("pawn", 1, 0, 1, 0, -1)
	
	# Start the game
	start_game()
	
	# Do 2 space move on second pawn
	# .  ♟️
	# .  .
	# ♟️ .
	assert_true(game_state.TakeActionAt(Vector2i(1,2), e_pawn))
	
	# Only Pawn 2 should have "pawn_initial"
	assert_false(pawn.HasTag("pawn_initial"))
	assert_true(e_pawn.HasTag("pawn_initial"))
	next_turn()
	
	# Make sure tag is still there on next turn
	assert_true(e_pawn.HasTag("pawn_initial"))

	next_turn()
	# Tag should be gone
	assert_false(e_pawn.HasTag("pawn_initial"))
	
	# Move Pawn 1 up 2 spaces
	# ♟️ ♟️
	# .  .
	# .  .
	assert_true(game_state.TakeActionAt(Vector2i(0,2), pawn))
	
	# Only Pawn 1 should have the tag
	assert_true(pawn.HasTag("pawn_initial"))
	assert_false(e_pawn.HasTag("pawn_initial"))
	
	# Move to next turn
	next_turn()
	
	# Pawn 1 should not be able to use En Passant, (but can also move + attack)
	if piece_has_actions_at(e_pawn, Vector2i(0,1), 2):
		fail_test("Pawn2 is still able to En Passant, when it shouldn't be able to.")




func test_enemy() -> void:
	
	# Initialise board
	# .  ♟️
	# .  .
	# ♟️ .
	var pawn = game_state.PlacePiece("pawn", 0, 0, 0, 0, -1)
	var e_pawn = game_state.PlacePiece("pawn", 0, 1, 1, 2, -1)
	
	# Start the game
	start_game()
	
	# Do 2 space move on first pawn
	# ♟️ ♟️
	# .  .
	# .  .
	assert_true(game_state.TakeActionAt(Vector2i(0,2), pawn))
	
	# Only Pawn 1 should have "pawn_initial"
	assert_true(pawn.HasTag("pawn_initial"))
	assert_false(e_pawn.HasTag("pawn_initial"))
	next_turn()
	# Make sure tag is still there on next turn
	assert_true(pawn.HasTag("pawn_initial"))
	
	# Enemy should be able to use En Passant
	if not piece_has_actions_at(e_pawn, Vector2i(0,1)):
		fail_test("Pawn2 is unable to En Passant")
	
	# Make sure pawn can En Passant
	# .  .
	# ♟️ .
	# .  .
	assert_true(game_state.TakeActionAt(Vector2i(0,1), e_pawn))
	# Pawn 1 should be gone, and Pawn 2 still there
	assert_false(game_state.allPieces.has(pawn))
	assert_true(game_state.allPieces.has(e_pawn))
	next_turn()
	# Tag should still be there, as pawn1 is no longer processed
	assert_true(pawn.HasTag("pawn_initial"))
	
	# Action should be gone
	if piece_has_actions_at(e_pawn, Vector2i(0,1)):
		fail_test("Action on Pawn 2's space, when there should not be one.")





func test_pawn_taken() -> void:
	
	# Skip through checks already done above
	# Initialise board
	# .  ♟️
	# .  ♗
	# ♟️ .
	var pawn = game_state.PlacePiece("pawn", 0, 0, 0, 0, -1)
	var e_pawn = game_state.PlacePiece("pawn", 1, 1, 1, 2, -1)
	var e_bishop = game_state.PlacePiece("bishop", 1, 1, 1, 1, -1)
	
	# Start the game
	start_game()
	
	# Do 2 space move on first pawn
	# ♟️ ♟️
	# .  ♗
	# .  .
	assert_true(game_state.TakeActionAt(Vector2i(0,2), pawn))
	
	next_turn()
	
	# Enemy should be able to use En Passant
	if not piece_has_actions_at(e_pawn, Vector2i(0,1)):
		fail_test("Pawn2 is unable to En Passant")
	
	# Pawn 1 is taken by the bishop
	# ♗ ♟️
	# .  .
	# .  .
	assert_true(game_state.TakeActionAt(Vector2i(0,2), e_bishop))
	# Pawn 1 should be gone, while Bishop and Pawn 2 are still there
	assert_false(game_state.allPieces.has(pawn))
	assert_true(game_state.allPieces.has(e_pawn))
	assert_true(game_state.allPieces.has(e_bishop))
	
	next_turn()
	
	# Enemy should not be able to use En Passant, and only be able to Move + Attack
	if piece_has_actions_at(e_pawn, Vector2i(0,1), 2):
		fail_test("Pawn2 is still able to En Passant, when it shouldn't be able to.")
	
	# Also try to act it out
	assert_false(game_state.TakeActionAt(Vector2i(0,1), e_pawn))
