extends GameTest

func test_move() -> void:
	
	# Place Piece
	# .
	# .
	# ♟️
	var pawn = place_piece("pawn", 0, 0, 0, 0)
	
	start_game()
	
	# Piece should be able to move forwards one space
	assert_true(piece_has_actions_at(pawn, 0, 1), "Pawn should have the action to move forward.")
	
	# Try to move forward
	# .
	# ♟️
	# .
	assert_true(piece_act_at(pawn, 0, 1), "Pawn should be able to move forward.")
	
	# Pawn should have moved forward one
	assert_true(piece_on_cell(pawn, 0, 1), "Pawn should have moved forward.")
	
	# Pawn should not have tag for initial move
	assert_false(piece_has_tag(pawn, "pawn_initial"), "Pawn should not have initial move tag.")
	
	# Skip to next turn
	next_turn(0)
	
	# Piece should be able to move forwards one space
	assert_true(piece_has_actions_at(pawn, 0, 2), "Pawn should have the action to move forward again.")
	
	# Try to move another space forwards
	# ♟️
	# .
	# .
	assert_true(piece_act_at(pawn, 0, 2), "Pawn should be able to move forward again.")
	
	# Pawn should have moved forward one
	assert_true(piece_on_cell(pawn, 0, 2), "Pawn should have moved forward again.")
	
	# Pawn should not have tag for initial move
	assert_false(piece_has_tag(pawn, "pawn_initial"), "Pawn should not have initial move tag.")



func test_move_blocked_team() -> void:
	
	# Initialise board
	# ♗
	# ♟️
	var pawn = place_piece("pawn", 0, 0, 0, 0)
	var bishop = place_piece("bishop", 0, 0, 0, 1)
	
	start_game()
	
	# Piece should be able to move forwards one space
	assert_true(piece_has_actions_at(pawn, 0,1), "Pawn should have the action to move forward.")
	
	# Try (and fail) to move forward
	# ♗
	# ♟️
	assert_false(piece_act_at(pawn, 0, 1), "Pawn should not be able to move forward.")
	
	# Pawn should have moved forward one
	assert_true(piece_on_cell(pawn, 0, 0), "Pawn should not have moved.")
	
	# Move the other piece out of the way
	# . ♗
	# ♟️  .
	move_piece(bishop, 1, 1)
	
	# Skip to next turn to update verification
	next_turn(0)
	
	# Try to move forward
	# ♟ ♗
	# . .
	assert_true(piece_act_at(pawn, 0, 1), "Pawn should be able to move forward.")
	
	# Pawn should have moved forward one
	assert_true(piece_on_cell(pawn, 0, 1), "Pawn should have moved forward.")

# Try to move normally when the initial 2 space move is blocked at the two space point
func test_move_initial_far_blocked_team() -> void:
	
	# Initialise board
	# ♗
	# .
	# ♟️
	var pawn = place_piece("pawn", 0, 0, 0, 0)
	var bishop = place_piece("bishop", 0, 0, 0, 2)
	
	start_game()
	
	# Piece should be able to move forwards one space
	assert_true(piece_has_actions_at(pawn, 0,1), "Pawn should have the action to move forward.")
	
	# Try to move forward
	# ♗
	# ♟️
	# .
	assert_true(piece_act_at(pawn, 0, 1), "Pawn should be able to move forward.")
	
	# Pawn should have moved forward one
	assert_true(piece_on_cell(pawn, 0, 1), "Pawn should have moved.")
	
	# Pawn should not have initial tag
	assert_false(piece_has_tag(pawn, "pawn_initial"), "Pawn should not have pawn_initial tag.")



func test_move_blocked_enemy() -> void:
	
	# Initialise board
	# ♗
	# ♟️
	var pawn = place_piece("pawn", 0, 0, 0, 0)
	var bishop = place_piece("bishop", 0, 1, 0, 1)
	
	start_game()
	
	# Piece should be able to move forwards one space
	assert_true(piece_has_actions_at(pawn, 0,1), "Pawn should have the action to move forward.")
	
	# Try (and fail) to move forward
	# ♗
	# ♟️
	assert_false(piece_act_at(pawn, 0, 1), "Pawn should not be able to move forward.")
	
	# Pawn should have moved forward one
	assert_true(piece_on_cell(pawn, 0, 0), "Pawn should not have moved.")
	
	# Move the other piece out of the way
	# . ♗
	# ♟️  .
	move_piece(bishop, 1, 1)
	
	# Skip to next turn to update verification
	next_turn(0)
	
	# Try to move forward
	# ♟ ♗
	# . .
	assert_true(piece_act_at(pawn, 0, 1), "Pawn should be able to move forward.")
	
	# Pawn should have moved forward one
	assert_true(piece_on_cell(pawn, 0, 1), "Pawn should have moved forward.")

# Try to move normally when the initial 2 space move is blocked at the two space point
func test_move_initial_far_blocked_enemy() -> void:
	
	# Initialise board
	# ♗
	# .
	# ♟️
	var pawn = place_piece("pawn", 0, 0, 0, 0)
	var bishop = place_piece("bishop", 0, 1, 0, 2)
	
	start_game()
	
	# Piece should be able to move forwards one space
	assert_true(piece_has_actions_at(pawn, 0, 1), "Pawn should have the action to move forward.")
	
	# Try to move forward
	# ♗
	# ♟️
	# .
	assert_true(piece_act_at(pawn, 0, 1), "Pawn should be able to move forward.")
	
	# Pawn should have moved forward one
	assert_true(piece_on_cell(pawn, 0,1), "Pawn should have moved.")
	
	# Pawn should not have initial tag
	assert_false(piece_has_tag(pawn, "pawn_initial"), "Pawn should not have pawn_initial tag.")




func test_initial_move() -> void:
	
	# Initialise board
	# . 
	# . 
	# ♟️
	var pawn = place_piece("pawn", 0, 0, 0, 0)
	
	# Start the game
	start_game()
	
	# Piece should be able to move forwards two spaces
	assert_true(piece_has_actions_at(pawn, 0, 2), "Pawn should have the action to move forward two spaces.")
	
	# Do 2 space move on second pawn
	# ♟️ 
	# . 
	# .
	assert_true(piece_act_at(pawn, 0,2), "Pawn should be able to move two spaces forwards.")
	
	assert_true(piece_on_cell(pawn, 0, 2), "Pawn should have moved.")
	
	# Pawn should have "pawn_initial"
	assert_true(piece_has_tag(pawn, "pawn_initial"), "Pawn should have pawn_initial tag (Own turn after move).")
	next_turn(1)
	
	# Pawn should still have "pawn_initial" as it's not that player's turn
	assert_true(piece_has_tag(pawn, "pawn_initial"), "Pawn should have pawn_initial tag (Enemy turn).")
	
	next_turn(0)
	
	# Pawn should no longer have the "pawn_initial" tag
	
	assert_false(piece_has_tag(pawn, "pawn_initial"), "Pawn should no longer have pawn_initial tag.")
	
	# Pawn should not be able to move two spaces up,
	# but should be able to move 1 space up
	assert_false(piece_act_at(pawn, 0, 4), "Pawn should not be able to move one spaces forward.")
	assert_true(piece_act_at(pawn, 0, 3), "Pawn should still be able to move one space forward.")



func test_initial_move_blocked_close_team() -> void:
	
	# Initialise board
	# . 
	# ♗
	# ♟️
	var pawn = place_piece("pawn", 0, 0, 0, 0)
	var bishop = place_piece("bishop", 0, 0, 0, 1)
	
	# Start the game
	start_game()
	
	# Piece should be able to move forwards two spaces
	assert_true(piece_has_actions_at(pawn, 0, 2), "Pawn should have the action to move forward two spaces.")
	
	# Try (and fail) to move 2 spaces forward
	# . 
	# ♗ 
	# ♟️
	assert_false(piece_act_at(pawn, 0, 2), "Pawn should not be able to move two spaces forwards.")
	
	assert_true(piece_on_cell(pawn, 0, 0), "Pawn should not have moved.")
	
	# Pawn should not have "pawn_initial"
	assert_false(piece_has_tag(pawn, "pawn_initial"), "Pawn should not have pawn_initial tag (Own turn after failed move).")
	
	# Move Bishop out of the way
	# . .
	# . ♗ 
	# ♟️ .
	move_piece(bishop, 1, 1)
	
	next_turn(1)
	
	# Pawn should still have "pawn_initial" as it's not that player's turn
	assert_false(piece_has_tag(pawn, "pawn_initial"), "Pawn should not have pawn_initial tag (Enemy turn after failed move).")
	
	next_turn(0)
	
	# Pawn should be able to move now
	# ♟️ .
	# . ♗ 
	# . .
	assert_true(piece_act_at(pawn, 0, 2), "Pawn should be able to move two spaces forwards.")
	assert_true(piece_has_tag(pawn, "pawn_initial"), "Pawn should have pawn_initial tag (Own turn move).")
	
	assert_true(piece_on_cell(pawn, 0, 2), "Pawn should have moved.")

func test_initial_move_blocked_far_team() -> void:
	
	# Initialise board
	# ♗
	# .
	# ♟️
	var pawn = place_piece("pawn", 0, 0, 0, 0)
	var bishop = place_piece("bishop", 0, 0, 0, 2)
	
	# Start the game
	start_game()
	
	# Piece should be able to move forwards two spaces
	assert_true(piece_has_actions_at(pawn, 0, 2), "Pawn should have the action to move forward two spaces.")
	
	# Try (and fail) to move 2 spaces forward
	# ♗ 
	# . 
	# ♟️
	assert_false(piece_act_at(pawn, 0, 2), "Pawn should not be able to move two spaces forwards.")
	
	assert_true(piece_on_cell(pawn, 0, 0), "Pawn should not have moved.")
	
	# Pawn should not have "pawn_initial"
	assert_false(piece_has_tag(pawn, "pawn_initial"), "Pawn should not have pawn_initial tag (Own turn after failed move).")
	
	# Move Bishop out of the way
	# . ♗
	# . . 
	# ♟️ .
	move_piece(bishop, 1, 2)
	
	next_turn(1)
	
	# Pawn should still have "pawn_initial" as it's not that player's turn
	assert_false(piece_has_tag(pawn, "pawn_initial"), "Pawn should not have pawn_initial tag (Enemy turn after failed move).")
	
	next_turn(0)
	
	# Pawn should be able to move now
	# ♟️ ♗
	# . . 
	# . .
	assert_true(piece_act_at(pawn, 0, 2), "Pawn should be able to move two spaces forwards.")
	assert_true(piece_has_tag(pawn, "pawn_initial"), "Pawn should have pawn_initial tag (Own turn move).")
	
	assert_true(piece_on_cell(pawn, 0, 2), "Pawn should have moved.")



func test_initial_move_blocked_close_enemy() -> void:
	
	# Initialise board
	# . 
	# ♗
	# ♟️
	var pawn = place_piece("pawn", 0, 0, 0, 0)
	var bishop = place_piece("bishop", 0, 1, 0, 1)
	
	# Start the game
	start_game()
	
	# Piece should be able to move forwards two spaces
	assert_true(piece_has_actions_at(pawn, 0, 2), "Pawn should have the action to move forward two spaces.")
	
	# Try (and fail) to move 2 spaces forward
	# . 
	# ♗ 
	# ♟️
	assert_false(piece_act_at(pawn, 0, 2), "Pawn should not be able to move two spaces forwards.")
	
	assert_true(piece_on_cell(pawn, 0, 0), "Pawn should not have moved.")
	
	# Pawn should not have "pawn_initial"
	assert_false(piece_has_tag(pawn, "pawn_initial"), "Pawn should not have pawn_initial tag (Own turn after failed move).")
	
	# Move Bishop out of the way
	# . .
	# . ♗ 
	# ♟️ .
	move_piece(bishop, 1, 1)
	
	next_turn(1)
	
	# Pawn should still have "pawn_initial" as it's not that player's turn
	assert_false(piece_has_tag(pawn, "pawn_initial"), "Pawn should not have pawn_initial tag (Enemy turn after failed move).")
	
	next_turn(0)
	
	# Pawn should be able to move now
	# ♟️ .
	# . ♗ 
	# . .
	assert_true(piece_act_at(pawn, 0, 2), "Pawn should be able to move two spaces forwards.")
	assert_true(piece_has_tag(pawn, "pawn_initial"), "Pawn should have pawn_initial tag (Own turn move).")
	
	assert_true(piece_on_cell(pawn, 0, 2), "Pawn should have moved.")

func test_initial_move_blocked_far_enemy() -> void:
	
	# Initialise board
	# ♗
	# .
	# ♟️
	var pawn = place_piece("pawn", 0, 0, 0, 0)
	var bishop = place_piece("bishop", 0, 1, 0, 2)
	
	# Start the game
	start_game()
	
	# Piece should be able to move forwards two spaces
	assert_true(piece_has_actions_at(pawn, 0, 2), "Pawn should have the action to move forward two spaces.")
	
	# Try (and fail) to move 2 spaces forward
	# ♗ 
	# . 
	# ♟️
	assert_false(piece_act_at(pawn, 0, 2), "Pawn should not be able to move two spaces forwards.")
	
	assert_true(piece_on_cell(pawn, 0, 0), "Pawn should not have moved.")
	
	# Pawn should not have "pawn_initial"
	assert_false(piece_has_tag(pawn, "pawn_initial"), "Pawn should not have pawn_initial tag (Own turn after failed move).")
	
	# Move Bishop out of the way
	# . ♗
	# . . 
	# ♟️ .
	move_piece(bishop, 1, 2)
	
	next_turn(1)
	
	# Pawn should still have "pawn_initial" as it's not that player's turn
	assert_false(piece_has_tag(pawn, "pawn_initial"), "Pawn should not have pawn_initial tag (Enemy turn after failed move).")
	
	next_turn(0)
	
	# Pawn should be able to move now
	# ♟️ ♗
	# . . 
	# . .
	assert_true(piece_act_at(pawn, 0, 2), "Pawn should be able to move two spaces forwards.")
	assert_true(piece_has_tag(pawn, "pawn_initial"), "Pawn should have pawn_initial tag (Own turn move).")
	
	assert_true(piece_on_cell(pawn, 0, 2), "Pawn should have moved.")




func test_initial_not_first_move() -> void:
	
	# Place Piece
	# .
	# .
	# ♟️
	var pawn = place_piece("pawn", 0, 0, 0, 0)
	
	# Set pawn to have moved at least once
	pawn.timesMoved = 1
	
	# Start the game
	start_game()
	# Piece should not have the actions to move forwards 2 spaces
	assert_false(piece_has_actions_at(pawn, 0, 2), "Pawn should have the action to move forward two spaces.")
	
	# Try (and fail) to move 2 spaces forward
	# . 
	# .
	# ♟️
	assert_false(piece_act_at(pawn, 0, 2), "Pawn should not be able to move two spaces forwards.")
	
	assert_true(piece_on_cell(pawn, 0, 0), "Pawn should not have moved.")




func test_attack_team_right() -> void:
	
	# Place pieces
	# . ♗
	# ♟️ .
	var pawn = place_piece("pawn", 0, 0, 0 , 0)
	var bishop = place_piece("bishop", 0, 0, 1 , 1)
	
	# Start the game
	start_game()
	
	# Piece should have the action to attack
	assert_true(piece_has_actions_at(pawn, 1, 1), "Pawn should have the action to attack diagonally.")
	
	# Pawn should not be able to attack
	assert_false(piece_act_at(pawn, 1, 1), "Pawn should not be able to attack team piece.")
	
	assert_true(piece_on_cell(pawn, 0, 0), "Pawn should not have moved.")
	
	assert_true(piece_on_board(bishop), "Bishop should still be in the game.")


func test_attack_team_left() -> void:
	
	# Place pieces
	# ♗  .
	# . ♟️
	var pawn = place_piece("pawn", 0, 0, 1, 0)
	var bishop = place_piece("bishop", 0, 0, 0, 1)
	
	# Start the game
	start_game()
	
	# Piece should have the action to attack
	assert_true(piece_has_actions_at(pawn, 0, 1), "Pawn should have the action to attack diagonally.")
	
	# Pawn should not be able to attack
	assert_false(piece_act_at(pawn, 0, 1), "Pawn should not be able to attack team piece.")
	
	assert_true(piece_on_cell(pawn, 1, 0), "Pawn should not have moved.")
	
	assert_true(piece_on_board(bishop), "Bishop should still be in the game.")



func test_attack_enemy_right() -> void:
	
	# Place pieces
	# . ♗
	# ♟️ .
	var pawn = place_piece("pawn", 0, 0, 0 , 0)
	var e_bishop = place_piece("bishop", 0, 1, 1 , 1)
	
	# Start the game
	start_game()
	
	# Piece should have the action to attack
	assert_true(piece_has_actions_at(pawn, 1, 1), "Pawn should have the action to attack diagonally.")
	
	# Pawn should not be able to attack
	assert_true(piece_act_at(pawn, 1, 1), "Pawn should be able to attack enemy piece.")
	
	assert_true(piece_on_cell(pawn, 1, 1), "Pawn should have moved.")
	
	assert_false(piece_on_board(e_bishop), "Bishop should not still be in the game.")
	


func test_attack_enemy_left() -> void:
	
	# Place pieces
	# ♗  .
	# . ♟️
	var pawn = place_piece("pawn", 0, 0, 1, 0)
	var e_bishop = place_piece("bishop", 0, 1, 0, 1)
	
	# Start the game
	start_game()
	
	# Piece should have the action to attack
	assert_true(piece_has_actions_at(pawn, 0, 1), "Pawn should have the action to attack diagonally.")
	
	# Pawn should not be able to attack
	assert_true(piece_act_at(pawn, 0, 1), "Pawn should be able to attack enemy piece.")
	
	assert_true(piece_on_cell(pawn, 0,1), "Pawn should have moved.")
	
	assert_false(piece_on_board(e_bishop), "Bishop should not still be in the game.")
