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



# When checking for En Passant actions, there should be
# 2 actions minimum as the pawn's attack actions should be present in addition
# to the En Passant ones.

func test_teammate() -> void:
	
	# Initialise board
	# .  .
	# .  .
	# ♟️ ♟️
	var pawn = game.place_piece("pawn", 0, 0, 0, 0)
	var pawn_2 = game.place_piece("pawn", 1, 0, 1, 0)
	
	# Start the game
	game.start_game()
	
	# Do 2 space move on second pawn
	# .  ♟️
	# .  .
	# ♟️ .
	assert_true(game.piece_act_at(pawn_2, 1, 2), "Pawn 2 should be able to move 2 spaces forward.")
	
	# Only Pawn 2 should have "pawn_initial"
	assert_false(game.piece_has_tag(pawn, "pawn_initial"), "Pawn 1 should not have pawn_initial tag.")
	assert_true(game.piece_has_tag(pawn_2, "pawn_initial"), "Pawn 2 should have pawn_initial tag.")
	game.next_turn(1)
	
	# Make sure tag is still there on next turn
	assert_true(game.piece_has_tag(pawn_2, "pawn_initial"), "Pawn 2 should still have pawn_initial on enemy turn.")

	game.next_turn(0)
	# Tag should be gone
	assert_false(game.piece_has_tag(pawn_2, "pawn_initial"), "Pawn 2 should no longer have pawn_initial tag.")
	
	# Move Pawn 1 up 2 spaces
	# ♟️ ♟️
	# .  .
	# .  .
	assert_true(game.piece_act_at(pawn, 0, 2), "Pawn 1 should be able to move 2 spaces forward.")
	
	# Only Pawn 1 should have the tag
	assert_true(game.piece_has_tag(pawn, "pawn_initial"), "Pawn 1 should be have pawn_initial tag.")
	assert_false(game.piece_has_tag(pawn_2, "pawn_initial"), "Pawn 2 should not have pawn_initial tag.")
	
	# Move to next turn
	game.next_turn()
	
	# Pawn 1 should not be able to use En Passant, (but can also move + attack)
	assert_false(game.piece_has_actions_at(pawn_2, 0, 1, 2), "Pawn 2 is still able to En Passant, when it shouldn't be able to.")




func test_enemy() -> void:
	
	# Initialise board
	# .  ♟️
	# .  .
	# ♟️ .
	var pawn = game.place_piece("pawn", 0, 0, 0, 0)
	var e_pawn = game.place_piece("pawn", 0, 1, 1, 2)
	
	# Start the game
	game.start_game()
	
	# Do 2 space move on first pawn
	# ♟️ ♟️
	# .  .
	# .  .
	assert_true(game.piece_act_at(pawn, 0, 2), "Pawn should be able to move 2 spaces forawrd.")
	
	# Only Pawn 1 should have "pawn_initial"
	assert_true(game.piece_has_tag(pawn, "pawn_initial"), "Pawn should have pawn_initial tag.")
	assert_false(game.piece_has_tag(e_pawn, "pawn_initial"), "Enemy Pawn should not have pawn_initial tag.")
	game.next_turn()
	# Make sure tag is still there on next turn
	assert_true(game.piece_has_tag(pawn, "pawn_initial"), "Pawn should still have pawn_initial tag on enemy's turn.")
	
	# Enemy should be able to use En Passant
	assert_true(game.piece_has_actions_at(e_pawn, 0, 1, 2), "Pawn2 is unable to En Passant")
	
	# Make sure pawn can En Passant
	# .  .
	# ♟️ .
	# .  .
	assert_true(game.piece_act_at(e_pawn, 0, 1), "Enemy Pawn should be able to use En Passant on the Pawn.")
	# Pawn 1 should be gone, and Pawn 2 still there
	assert_false(game.piece_on_board(pawn), "Pawn should have been removed from the board.")
	assert_true(game.piece_on_board(e_pawn), "Enemy Pawn should still be on the board.")
	
	game.next_turn()
	# Tag should still be there, as the pawn is no longer processed
	assert_true(game.piece_has_tag(pawn, "pawn_initial"), "Pawn should have the initial tag.")
	
	# Action should be gone
	assert_false(game.piece_has_actions_at(e_pawn, 0, 1), "Action on Pawn 2's space, when there should not be one.")





func test_pawn_taken() -> void:
	
	# Skip through checks already done above
	# Initialise board
	# .  ♟️
	# .  ♗
	# ♟️ .
	var pawn = game.place_piece("pawn", 0, 0, 0, 0)
	var e_pawn = game.place_piece("pawn", 1, 1, 1, 2)
	var e_bishop = game.place_piece("bishop", 1, 1, 1, 1)
	
	# Start the game
	game.start_game()
	
	# Do 2 space move on first pawn
	# ♟️ ♟️
	# .  ♗
	# .  .
	assert_true(game.piece_act_at(pawn, 0, 2))
	
	game.next_turn()
	
	# Enemy should be able to use En Passant
	assert_true(game.piece_has_actions_at(e_pawn, 0, 1, 2), "Pawn2 is unable to En Passant")
	
	# Pawn 1 is taken by the bishop
	# ♗ ♟️
	# .  .
	# .  .
	assert_true(game.piece_act_at(e_bishop, 0, 2), "Bishop couldn't take the Pawn.")
	# Pawn 1 should be gone, while Bishop and Pawn 2 are still there
	assert_false(game.piece_on_board(pawn), "Pawn should not be on the board")
	assert_true(game.piece_on_board(e_pawn), "Enemy Pawn should still be on the board")
	assert_true(game.piece_on_board(e_bishop), "Enemy Bishop should still be on the board")
	
	game.next_turn()
	
	# Enemy should not be able to use En Passant, and only be able to Move + Attack
	assert_false(game.piece_has_actions_at(e_pawn, 0, 1, 2), "Pawn2 is still able to En Passant, when it shouldn't be able to.")
	
	# Also try to act it out
	assert_false(game.piece_act_at(e_pawn, 0, 1), "Enemy Pawn should not be able to En Passant")
