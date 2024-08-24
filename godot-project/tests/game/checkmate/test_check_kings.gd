extends GameTest


# No pieces at all (should be in checkmate)
func test_nothing() -> void:
	
	# Place nothing on the board, and start the game
	start_game_with_seed(game_state.gameRandom.seed)
	
	# Both players should immediately be in check
	assert_true(game_state.PlayerHasNoKing(0))
	assert_true(game_state.PlayerHasNoKing(1))




# No pieces but two kings. Taking one should result in check
func test_kings() -> void:
	
	# Place two Kings on the board
	# .  .  👑  
	# .  .  .
	# 👑 .  .
	var king = place_piece("king", 0, 0, 0, 0)
	var e_king = place_piece("king", 0, 1, 2, 2)
	
	start_game_with_seed(game_state.gameRandom.seed)
	
	# Neither should be in check
	assert_false(game_state.PlayerInCheck(0))
	assert_false(game_state.PlayerInCheck(1))
	
	# Move King on team 0 to the middle
	# Has to be forced as the Action is invalid
	# .  .  👑  
	# .  👑 .
	# .  .  .
	move_piece(king, 1, 1)
	
	next_turn(1)
	
	# Both players should be in check
	assert_true(game_state.PlayerInCheck(0))
	assert_true(game_state.PlayerInCheck(1))
	
	# Removing one King should result in that king
	# "check" (having check set to "NoKing"), and the other not.
	# .  .  .  
	# .  👑 .
	# .  .  .
	take_piece(e_king, king)
	
	next_turn(0)
	
	# Neither should be in check, but player 2 should have no king
	assert_false(game_state.PlayerInCheck(0))
	assert_false(game_state.PlayerInCheck(1))
	assert_true(game_state.PlayerHasNoKing(1))
