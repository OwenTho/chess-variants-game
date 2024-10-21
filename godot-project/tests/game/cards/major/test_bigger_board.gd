extends GutTest

var card_script: CSharpScript = preload("res://scripts/game/card/major/BiggerBoardCard.cs")
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



func test_card() -> void:
	
	var card = card_script.new()
	card.CardId = "bigger_board"
	
	# Place 2 pieces on the board, at the bottom left corner
	var bottom_corner: Vector2i = game.game_controller.GridLowerCorner
	var queen = game.place_piece("queen", 0, 0, 0, 0)
	var e_rook = game.place_piece("rook", 0, 1, 0, 1)
	
	game.start_game()
	
	# Neither piece should be able to move off the board
	assert_false(game.piece_act_at(queen, -1, 0), "Queen should not be able to act off the board (-1, 0).")
	assert_false(game.piece_act_at(queen, -2, 0), "Queen should not be able to act off the board (-2, 0).")
	assert_false(game.piece_act_at(queen, -1, -1), "Queen should not be able to act off the board (-1, -1).")
	assert_false(game.piece_act_at(queen, -2, -2), "Queen should not be able to act off the board (-2, -2).")
	game.move_piece(queen, 0, 0) # Move the queen back just in case
	game.next_turn(1)
	assert_false(game.piece_act_at(e_rook, -1, 1), "Enemy Rook should not be able to act off the board (-1, 1).")
	assert_false(game.piece_act_at(e_rook, -2, 1), "Enemy Rook should not be able to act off the board (-2, 1).")
	game.move_piece(e_rook, 0, 1) # Move the enemy rook back just in case
	
	# Add the card
	game.add_card(card)
	
	# Swap turn over to update move validation
	game.next_turn(0)
	
	
	# The pieces should now be able to act off the board
	assert_true(game.piece_act_at(queen, -1, 0), "Queen should be able to act off the board (-1, 0).")
	assert_true(game.piece_act_at(queen, -2, 0), "Queen should be able to act off the board (-2, 0).")
	assert_true(game.piece_act_at(queen, -1, -1), "Queen should be able to act off the board (-1, -1).")
	assert_true(game.piece_act_at(queen, -2, -2), "Queen should be able to act off the board (-2, -2).")
	game.move_piece(queen, -2, 0)
	game.next_turn(1)
	assert_true(game.piece_act_at(e_rook, -1, 1), "Enemy Rook should be able to act off the board (-1, 1).")
	assert_true(game.piece_act_at(e_rook, -2, 1), "Enemy Rook should be able to act off the board (-2, 1).")
	game.move_piece(e_rook, -2, 1)
	
	# Queen should be able to take enemy rook off the board.
	game.next_turn(0)
	assert_true(game.piece_act_at(queen, -2, 1), "Queen should be able to act to move to (-2, 1).")
	assert_false(game.piece_on_board(e_rook), "Enemy Rook should no longer be on the board.")
