extends GameTest

func test_move() -> void:
	
	# Place Piece
	# .
	# .
	# ♟️
	var pawn = game_state.PlacePiece("pawn", 0, 0, 0, 0, -1)
	
	game_controller.StartGame()
	
	# Piece should be able to move forwards one space
	if not piece_has_actions_at(pawn, Vector2i(0,1)):
		fail_test("Pawn should have the action to move forward.")
	
	# Try to move forward
	# .
	# ♟️
	# .
	assert_true(game_state.TakeActionAt(Vector2i(0,1), pawn), "Pawn should be able to move forward.")
	
	# Pawn should have moved forward one
	assert_eq(pawn.cell.pos, Vector2i(0,1), "Pawn should have moved forward.")
	
	# Pawn should not have tag for initial move
	assert_false(pawn.HasTag("pawn_initial"), "Pawn should not have initial move tag.")
	
	# Skip to next turn
	game_state.NextTurn()
	game_state.NextTurn()
	
	# Piece should be able to move forwards one space
	if not piece_has_actions_at(pawn, Vector2i(0,2)):
		fail_test("Pawn should have the action to move forward again.")
	
	# Try to move another space forwards
	# ♟️
	# .
	# .
	assert_true(game_state.TakeActionAt(Vector2i(0,2), pawn), "Pawn should be able to move forward again.")
	
	# Pawn should have moved forward one
	assert_eq(pawn.cell.pos, Vector2i(0,2), "Pawn should have moved forward again.")
	
	# Pawn should not have tag for initial move
	assert_false(pawn.HasTag("pawn_initial"), "Pawn should not have initial move tag.")



func test_move_blocked_team() -> void:
	
	# Initialise board
	# ♗
	# ♟️
	var pawn = game_state.PlacePiece("pawn", 0, 0, 0, 0, -1)
	var bishop = game_state.PlacePiece("bishop", 0, 0, 0, 1, -1)
	
	game_controller.StartGame()
	
	# Piece should be able to move forwards one space
	if not piece_has_actions_at(pawn, Vector2i(0,1)):
		fail_test("Pawn should have the action to move forward.")
	
	# Try (and fail) to move forward
	# ♗
	# ♟️
	assert_false(game_state.TakeActionAt(Vector2i(0,1), pawn), "Pawn should not be able to move forward.")
	
	# Pawn should have moved forward one
	assert_eq(pawn.cell.pos, Vector2i(0,0), "Pawn should not have moved.")
	
	# Move the other piece out of the way
	# . ♗
	# ♟️  .
	game_state.MovePiece(bishop, 1, 1)
	
	# Skip to next turn to update verification
	game_state.NextTurn()
	game_state.NextTurn()
	
	# Try to move forward
	# ♟ ♗
	# . .
	assert_true(game_state.TakeActionAt(Vector2i(0,1), pawn), "Pawn should be able to move forward.")
	
	# Pawn should have moved forward one
	assert_eq(pawn.cell.pos, Vector2i(0,1), "Pawn should have moved forward.")

# Try to move normally when the initial 2 space move is blocked at the two space point
func test_move_initial_far_blocked_team() -> void:
	
	# Initialise board
	# ♗
	# .
	# ♟️
	var pawn = game_state.PlacePiece("pawn", 0, 0, 0, 0, -1)
	var bishop = game_state.PlacePiece("bishop", 0, 0, 0, 2, -1)
	
	game_controller.StartGame()
	
	# Piece should be able to move forwards one space
	if not piece_has_actions_at(pawn, Vector2i(0,1)):
		fail_test("Pawn should have the action to move forward.")
	
	# Try to move forward
	# ♗
	# ♟️
	# .
	assert_true(game_state.TakeActionAt(Vector2i(0,1), pawn), "Pawn should be able to move forward.")
	
	# Pawn should have moved forward one
	assert_eq(pawn.cell.pos, Vector2i(0,1), "Pawn should have moved.")
	
	# Pawn should not have initial tag
	assert_false(pawn.HasTag("pawn_initial"), "Pawn should not have pawn_initial tag.")



func test_move_blocked_enemy() -> void:
	
	# Initialise board
	# ♗
	# ♟️
	var pawn = game_state.PlacePiece("pawn", 0, 0, 0, 0, -1)
	var bishop = game_state.PlacePiece("bishop", 0, 1, 0, 1, -1)
	
	game_controller.StartGame()
	
	# Piece should be able to move forwards one space
	if not piece_has_actions_at(pawn, Vector2i(0,1)):
		fail_test("Pawn should have the action to move forward.")
	
	# Try (and fail) to move forward
	# ♗
	# ♟️
	assert_false(game_state.TakeActionAt(Vector2i(0,1), pawn), "Pawn should not be able to move forward.")
	
	# Pawn should have moved forward one
	assert_eq(pawn.cell.pos, Vector2i(0,0), "Pawn should not have moved.")
	
	# Move the other piece out of the way
	# . ♗
	# ♟️  .
	game_state.MovePiece(bishop, 1, 1)
	
	# Skip to next turn to update verification
	game_state.NextTurn()
	game_state.NextTurn()
	
	# Try to move forward
	# ♟ ♗
	# . .
	assert_true(game_state.TakeActionAt(Vector2i(0,1), pawn), "Pawn should be able to move forward.")
	
	# Pawn should have moved forward one
	assert_eq(pawn.cell.pos, Vector2i(0,1), "Pawn should have moved forward.")

# Try to move normally when the initial 2 space move is blocked at the two space point
func test_move_initial_far_blocked_enemy() -> void:
	
	# Initialise board
	# ♗
	# .
	# ♟️
	var pawn = game_state.PlacePiece("pawn", 0, 0, 0, 0, -1)
	var bishop = game_state.PlacePiece("bishop", 0, 1, 0, 2, -1)
	
	game_controller.StartGame()
	
	# Piece should be able to move forwards one space
	if not piece_has_actions_at(pawn, Vector2i(0,1)):
		fail_test("Pawn should have the action to move forward.")
	
	# Try to move forward
	# ♗
	# ♟️
	# .
	assert_true(game_state.TakeActionAt(Vector2i(0,1), pawn), "Pawn should be able to move forward.")
	
	# Pawn should have moved forward one
	assert_eq(pawn.cell.pos, Vector2i(0,1), "Pawn should have moved.")
	
	# Pawn should not have initial tag
	assert_false(pawn.HasTag("pawn_initial"), "Pawn should not have pawn_initial tag.")




func test_initial_move() -> void:
	
	# Initialise board
	# . 
	# . 
	# ♟️
	var pawn = game_state.PlacePiece("pawn", 0, 0, 0, 0, -1)
	
	# Start the game
	game_controller.StartGame()
	# Piece should be able to move forwards two spaces
	if not piece_has_actions_at(pawn, Vector2i(0,2)):
		fail_test("Pawn should have the action to move forward two spaces.")
	
	# Do 2 space move on second pawn
	# ♟️ 
	# . 
	# .
	assert_true(game_state.TakeActionAt(Vector2i(0,2), pawn), "Pawn should be able to move two spaces forwards.")
	
	assert_eq(pawn.cell.pos, Vector2i(0,2), "Pawn should have moved.")
	
	# Pawn should have "pawn_initial"
	assert_true(pawn.HasTag("pawn_initial"), "Pawn should have pawn_initial tag (Own turn after move).")
	game_state.NextTurn()
	
	# Pawn should still have "pawn_initial" as it's not that player's turn
	assert_true(pawn.HasTag("pawn_initial"), "Pawn should have pawn_initial tag (Enemy turn).")
	
	game_state.NextTurn()
	
	# Pawn should no longer have the "pawn_initial" tag
	
	assert_false(pawn.HasTag("pawn_initial"), "Pawn should no longer have pawn_initial tag.")
	
	# Pawn should not be able to move two spaces up,
	# but should be able to move 1 space up
	assert_false(game_state.TakeActionAt(Vector2i(0,4), pawn), "Pawn should not be able to move one spaces forward.")
	assert_true(game_state.TakeActionAt(Vector2i(0,3), pawn), "Pawn should still be able to move one space forward.")



func test_initial_move_blocked_close_team() -> void:
	
	# Initialise board
	# . 
	# ♗
	# ♟️
	var pawn = game_state.PlacePiece("pawn", 0, 0, 0, 0, -1)
	var bishop = game_state.PlacePiece("bishop", 0, 0, 0, 1, -1)
	
	# Start the game
	game_controller.StartGame()
	
	# Piece should be able to move forwards two spaces
	if not piece_has_actions_at(pawn, Vector2i(0,2)):
		fail_test("Pawn should have the action to move forward two spaces.")
	
	# Try (and fail) to move 2 spaces forward
	# . 
	# ♗ 
	# ♟️
	assert_false(game_state.TakeActionAt(Vector2i(0,2), pawn), "Pawn should not be able to move two spaces forwards.")
	
	assert_eq(pawn.cell.pos, Vector2i(0,0), "Pawn should not have moved.")
	
	# Pawn should not have "pawn_initial"
	assert_false(pawn.HasTag("pawn_initial"), "Pawn should not have pawn_initial tag (Own turn after failed move).")
	
	# Move Bishop out of the way
	# . .
	# . ♗ 
	# ♟️ .
	game_state.MovePiece(bishop, 1, 1)
	
	game_state.NextTurn()
	
	# Pawn should still have "pawn_initial" as it's not that player's turn
	assert_false(pawn.HasTag("pawn_initial"), "Pawn should not have pawn_initial tag (Enemy turn after failed move).")
	
	game_state.NextTurn()
	
	# Pawn should be able to move now
	# ♟️ .
	# . ♗ 
	# . .
	assert_true(game_state.TakeActionAt(Vector2i(0,2), pawn), "Pawn should be able to move two spaces forwards.")
	assert_true(pawn.HasTag("pawn_initial"), "Pawn should have pawn_initial tag (Own turn move).")
	
	assert_eq(pawn.cell.pos, Vector2i(0,2), "Pawn should have moved.")

func test_initial_move_blocked_far_team() -> void:
	
	# Initialise board
	# ♗
	# .
	# ♟️
	var pawn = game_state.PlacePiece("pawn", 0, 0, 0, 0, -1)
	var bishop = game_state.PlacePiece("bishop", 0, 0, 0, 2, -1)
	
	# Start the game
	game_controller.StartGame()
	
	# Piece should be able to move forwards two spaces
	if not piece_has_actions_at(pawn, Vector2i(0,2)):
		fail_test("Pawn should have the action to move forward two spaces.")
	
	# Try (and fail) to move 2 spaces forward
	# ♗ 
	# . 
	# ♟️
	assert_false(game_state.TakeActionAt(Vector2i(0,2), pawn), "Pawn should not be able to move two spaces forwards.")
	
	assert_eq(pawn.cell.pos, Vector2i(0,0), "Pawn should not have moved.")
	
	# Pawn should not have "pawn_initial"
	assert_false(pawn.HasTag("pawn_initial"), "Pawn should not have pawn_initial tag (Own turn after failed move).")
	
	# Move Bishop out of the way
	# . ♗
	# . . 
	# ♟️ .
	game_state.MovePiece(bishop, 1, 2)
	
	game_state.NextTurn()
	
	# Pawn should still have "pawn_initial" as it's not that player's turn
	assert_false(pawn.HasTag("pawn_initial"), "Pawn should not have pawn_initial tag (Enemy turn after failed move).")
	
	game_state.NextTurn()
	
	# Pawn should be able to move now
	# ♟️ ♗
	# . . 
	# . .
	assert_true(game_state.TakeActionAt(Vector2i(0,2), pawn), "Pawn should be able to move two spaces forwards.")
	assert_true(pawn.HasTag("pawn_initial"), "Pawn should have pawn_initial tag (Own turn move).")
	
	assert_eq(pawn.cell.pos, Vector2i(0,2), "Pawn should have moved.")



func test_initial_move_blocked_close_enemy() -> void:
	
	# Initialise board
	# . 
	# ♗
	# ♟️
	var pawn = game_state.PlacePiece("pawn", 0, 0, 0, 0, -1)
	var bishop = game_state.PlacePiece("bishop", 0, 1, 0, 1, -1)
	
	# Start the game
	game_controller.StartGame()
	# Piece should be able to move forwards two spaces
	if not piece_has_actions_at(pawn, Vector2i(0,2)):
		fail_test("Pawn should have the action to move forward two spaces.")
	
	# Try (and fail) to move 2 spaces forward
	# . 
	# ♗ 
	# ♟️
	assert_false(game_state.TakeActionAt(Vector2i(0,2), pawn), "Pawn should not be able to move two spaces forwards.")
	
	assert_eq(pawn.cell.pos, Vector2i(0,0), "Pawn should not have moved.")
	
	# Pawn should not have "pawn_initial"
	assert_false(pawn.HasTag("pawn_initial"), "Pawn should not have pawn_initial tag (Own turn after failed move).")
	
	# Move Bishop out of the way
	# . .
	# . ♗ 
	# ♟️ .
	game_state.MovePiece(bishop, 1, 1)
	
	game_state.NextTurn()
	
	# Pawn should still have "pawn_initial" as it's not that player's turn
	assert_false(pawn.HasTag("pawn_initial"), "Pawn should not have pawn_initial tag (Enemy turn after failed move).")
	
	game_state.NextTurn()
	
	# Pawn should be able to move now
	# ♟️ .
	# . ♗ 
	# . .
	assert_true(game_state.TakeActionAt(Vector2i(0,2), pawn), "Pawn should be able to move two spaces forwards.")
	assert_true(pawn.HasTag("pawn_initial"), "Pawn should have pawn_initial tag (Own turn move).")
	
	assert_eq(pawn.cell.pos, Vector2i(0,2), "Pawn should have moved.")

func test_initial_move_blocked_far_enemy() -> void:
	
	# Initialise board
	# ♗
	# .
	# ♟️
	var pawn = game_state.PlacePiece("pawn", 0, 0, 0, 0, -1)
	var bishop = game_state.PlacePiece("bishop", 0, 1, 0, 2, -1)
	
	# Start the game
	game_controller.StartGame()
	# Piece should be able to move forwards two spaces
	if not piece_has_actions_at(pawn, Vector2i(0,2)):
		fail_test("Pawn should have the action to move forward two spaces.")
	
	# Try (and fail) to move 2 spaces forward
	# ♗ 
	# . 
	# ♟️
	assert_false(game_state.TakeActionAt(Vector2i(0,2), pawn), "Pawn should not be able to move two spaces forwards.")
	
	assert_eq(pawn.cell.pos, Vector2i(0,0), "Pawn should not have moved.")
	
	# Pawn should not have "pawn_initial"
	assert_false(pawn.HasTag("pawn_initial"), "Pawn should not have pawn_initial tag (Own turn after failed move).")
	
	# Move Bishop out of the way
	# . ♗
	# . . 
	# ♟️ .
	game_state.MovePiece(bishop, 1, 2)
	
	game_state.NextTurn()
	
	# Pawn should still have "pawn_initial" as it's not that player's turn
	assert_false(pawn.HasTag("pawn_initial"), "Pawn should not have pawn_initial tag (Enemy turn after failed move).")
	
	game_state.NextTurn()
	
	# Pawn should be able to move now
	# ♟️ ♗
	# . . 
	# . .
	assert_true(game_state.TakeActionAt(Vector2i(0,2), pawn), "Pawn should be able to move two spaces forwards.")
	assert_true(pawn.HasTag("pawn_initial"), "Pawn should have pawn_initial tag (Own turn move).")
	
	assert_eq(pawn.cell.pos, Vector2i(0,2), "Pawn should have moved.")




func test_initial_not_first_move() -> void:
	
	# Place Piece
	# .
	# .
	# ♟️
	var pawn = game_state.PlacePiece("pawn", 0, 0, 0, 0, -1)
	
	# Set pawn to have moved at least once
	pawn.timesMoved = 1
	
	# Start the game
	game_controller.StartGame()
	# Piece should not have the actions to move forwards 2 spaces
	if piece_has_actions_at(pawn, Vector2i(0,2)):
		fail_test("Pawn should have the action to move forward two spaces.")
	
	# Try (and fail) to move 2 spaces forward
	# . 
	# .
	# ♟️
	assert_false(game_state.TakeActionAt(Vector2i(0,2), pawn), "Pawn should not be able to move two spaces forwards.")
	
	assert_eq(pawn.cell.pos, Vector2i(0,0), "Pawn should not have moved.")




func test_attack_team_right() -> void:
	
	# Place pieces
	# . ♗
	# ♟️ .
	var pawn = game_state.PlacePiece("pawn", 0, 0, 0 , 0, -1)
	var bishop = game_state.PlacePiece("bishop", 0, 0, 1 , 1, -1)
	
	# Start the game
	game_controller.StartGame()
	
	# Piece should have the action to attack
	if not piece_has_actions_at(pawn, Vector2i(1,1)):
		fail_test("Pawn should have the action to attack diagonally.")
	
	# Pawn should not be able to attack
	assert_false(game_state.TakeActionAt(Vector2i(1,1), pawn), "Pawn should not be able to attack team piece.")
	
	assert_eq(pawn.cell.pos, Vector2i(0,0), "Pawn should not have moved.")
	
	assert_true(game_state.allPieces.has(bishop), "Bishop should still be in the game.")


func test_attack_team_left() -> void:
	
	# Place pieces
	# ♗  .
	# . ♟️
	var pawn = game_state.PlacePiece("pawn", 0, 0, 1, 0, -1)
	var bishop = game_state.PlacePiece("bishop", 0, 0, 0, 1, -1)
	
	# Start the game
	game_controller.StartGame()
	
	# Piece should have the action to attack
	if not piece_has_actions_at(pawn, Vector2i(0,1)):
		fail_test("Pawn should have the action to attack diagonally.")
	
	# Pawn should not be able to attack
	assert_false(game_state.TakeActionAt(Vector2i(0,1), pawn), "Pawn should not be able to attack team piece.")
	
	assert_eq(pawn.cell.pos, Vector2i(1,0), "Pawn should not have moved.")
	
	assert_true(game_state.allPieces.has(bishop), "Bishop should still be in the game.")



func test_attack_enemy_right() -> void:
	
	# Place pieces
	# . ♗
	# ♟️ .
	var pawn = game_state.PlacePiece("pawn", 0, 0, 0 , 0, -1)
	var e_bishop = game_state.PlacePiece("bishop", 0, 1, 1 , 1, -1)
	
	# Start the game
	game_controller.StartGame()
	
	# Piece should have the action to attack
	if not piece_has_actions_at(pawn, Vector2i(1,1)):
		fail_test("Pawn should have the action to attack diagonally.")
	
	# Pawn should not be able to attack
	assert_true(game_state.TakeActionAt(Vector2i(1,1), pawn), "Pawn should be able to attack enemy piece.")
	
	assert_eq(pawn.cell.pos, Vector2i(1,1), "Pawn should have moved.")
	
	assert_false(game_state.allPieces.has(e_bishop), "Bishop should not still be in the game.")
	


func test_attack_enemy_left() -> void:
	
	# Place pieces
	# ♗  .
	# . ♟️
	var pawn = game_state.PlacePiece("pawn", 0, 0, 1, 0, -1)
	var e_bishop = game_state.PlacePiece("bishop", 0, 1, 0, 1, -1)
	
	# Start the game
	game_controller.StartGame()
	
	# Piece should have the action to attack
	if not piece_has_actions_at(pawn, Vector2i(0,1)):
		fail_test("Pawn should have the action to attack diagonally.")
	
	# Pawn should not be able to attack
	assert_true(game_state.TakeActionAt(Vector2i(0,1), pawn), "Pawn should be able to attack enemy piece.")
	
	assert_eq(pawn.cell.pos, Vector2i(0,1), "Pawn should have moved.")
	
	assert_false(game_state.allPieces.has(e_bishop), "Bishop should not still be in the game.")
