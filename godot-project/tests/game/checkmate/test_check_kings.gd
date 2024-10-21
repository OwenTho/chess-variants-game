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



# No pieces at all (should be in checkmate)
func test_nothing() -> void:
	
	# Place nothing on the board, and start the game
	game.start_game_with_seed(game.game_state.GameRandom.seed)
	
	# Both players should immediately be in check
	assert_true(game.game_state.PlayerHasNoKing(0), "Player 0 should have the 'NoKing' state.")
	assert_false(game.game_state.PlayerInCheck(0), "Player 0 should not be in Check.")
	assert_true(game.game_state.PlayerHasNoKing(1), "Player 1 should have the 'NoKing' state.")
	assert_false(game.game_state.PlayerInCheck(1), "Player 1 should not be in Check.")




# No pieces but two kings. Taking one should result in check
func test_kings() -> void:
	
	# Place two Kings on the board
	# .  .  👑  
	# .  .  .
	# 👑 .  .
	var king = game.place_piece("king", 0, 0, 0, 0)
	var e_king = game.place_piece("king", 0, 1, 2, 2)
	
	game.start_game_with_seed(game.game_state.GameRandom.seed)
	
	# Neither should be in check
	assert_false(game.game_state.PlayerInCheck(0), "Player 0 should not be in Check.")
	assert_false(game.game_state.PlayerInCheck(1), "Player 1 should not be in Check.")
	
	# Move King on team 0 to the middle
	# Has to be forced as the Action is invalid
	# .  .  👑  
	# .  👑 .
	# .  .  .
	game.move_piece(king, 1, 1)
	
	game.next_turn(1)
	
	# Both players should be in check
	assert_true(game.game_state.PlayerInCheck(0), "Player 1 should be in Check.")
	assert_true(game.game_state.PlayerInCheck(1), "Player 2 should be in Check.")
	
	# Removing one King should result in that king
	# "check" (having check set to "NoKing"), and the other not.
	# .  .  .  
	# .  👑 .
	# .  .  .
	game.take_piece(e_king, king)
	
	game.next_turn(0)
	
	# Neither should be in check, but player 2 should have no king
	assert_false(game.game_state.PlayerInCheck(0), "Player 0 should not be in Check.")
	assert_false(game.game_state.PlayerInCheck(1), "Player 1 should not be in Check.")
	assert_true(game.game_state.PlayerHasNoKing(1), "Player 1 should have the 'NoKing' state.")
