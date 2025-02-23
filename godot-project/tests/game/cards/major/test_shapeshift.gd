extends GutTest

var card_script: CSharpScript = preload("res://scripts/game/card/major/ShapeshiftCard.cs")
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
	card.CardId = "shapeshift"
	
	# Create a simple board of pieces
	#  . 🐴🤴.
	# ♟️  .   . ♗
	var pawn = game.place_piece("pawn", 0, 0, 0, 0)
	var bishop = game.place_piece("bishop", 0, 0, 3, 0)
	var e_knight = game.place_piece("knight", 0, 1, 1, 1)
	var e_queen = game.place_piece("queen", 0, 1, 2, 1)
	
	game.start_game()
	
	# Add the card
	game.add_card(card)
	
	# Nothing should have changed
	assert_true(game.piece_has_piece_id(pawn, "pawn"), "Pawn should still be a pawn.")
	assert_true(game.piece_has_piece_id(bishop, "bishop"), "Bishop should still be a bishop.")
	assert_true(game.piece_has_piece_id(e_knight, "knight"), "Enemy Knight should still be a knight.")
	assert_true(game.piece_has_piece_id(e_queen, "queen"), "Enemy Queen should still be a queen.")
	
	# First, the pawn should take the knight
	assert_true(game.piece_act_at(pawn, 1, 1), "Pawn should be able to take actions at (1,1)")
	
	# The pawn should now be a knight
	assert_true(game.piece_has_piece_id(pawn, "knight"), "Pawn should have changed into a knight.")
	
	# On the queen's turn, take the pawn that just changed into a knight
	game.next_turn(1)
	
	assert_true(game.piece_has_actions_at(e_queen, 3, 0), "Enemy Queen should have actions at (3,0) from (1,1), as it should be a knight.")
	assert_true(game.piece_act_at(e_queen, 1, 1), "Enemy Queen should be able to take actions at (1,1)")
	
	# The queen should now be a knight
	assert_true(game.piece_has_piece_id(e_queen, "knight"), "Enemy Queen should have changed into a knight.")
	
	# Skip back to player 2's turn
	game.next_turn(1)
	
	assert_true(game.piece_has_actions_at(e_queen, 3, 0), "Enemy Queen->Knight should have actions at (3,0) from (1,1), as it should be a knight.")
	
	# Player 2 should be able to take the bishop with the knight
	assert_true(game.piece_act_at(e_queen, 3, 0), "Enemy Queen->Knight should be able to move to (3,0)")
	
	# The queen should now be a bishop
	assert_true(game.piece_has_piece_id(e_queen, "bishop"), "Enemy Queen->Knight should have changed into a bishop.")
