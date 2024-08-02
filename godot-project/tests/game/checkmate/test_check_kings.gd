extends GameTest


# No pieces at all (should be in checkmate)
func test_nothing() -> void:
	
	# Place nothing on the board, and start the game
	game_controller.StartGame(game_state.gameRandom.seed)
	
	# Both players should immediately be in check
	assert_true(game_state.PlayerInCheck(0))
	assert_true(game_state.PlayerInCheck(1))

# No pieces but two kings. Taking one should result in check
func test_kings() -> void:
	
	# Place two Kings on the board
	# .  .  👑  
	# .  .  .
	# 👑 .  .
	var king1 = game_state.PlacePiece("king", 0, 0, 0, 0, -1)
	var king2 = game_state.PlacePiece("king", 0, 1, 2, 2, -1)
	
	game_controller.StartGame(game_state.gameRandom.seed)
	
	# Neither should be in check
	assert_false(game_state.PlayerInCheck(0))
	assert_false(game_state.PlayerInCheck(1))
	
	# Move King on team 0 to the middle
	# Has to be forced as the Action is invalid
	# .  .  👑  
	# .  👑 .
	# .  .  .
	game_state.MovePiece(king1, 1, 1)
	king1.EnableActionsUpdate();
	
	game_state.NextTurn()
	
	# Both players should be in check
	assert_true(game_state.PlayerInCheck(0))
	assert_true(game_state.PlayerInCheck(1))
	
	# Removing one King should result in that king
	# being in check, and the other not.
	game_state.TakePiece(king2, king1)
	
	game_state.NextTurn()
	
	# Team 0 not in check, Team 1 in check
	assert_false(game_state.PlayerInCheck(0))
	assert_true(game_state.PlayerInCheck(1))
