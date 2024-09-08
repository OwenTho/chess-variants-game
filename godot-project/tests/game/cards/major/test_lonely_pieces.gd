extends GameTest

var card_script: CSharpScript = preload("res://scripts/game/card/major/LonelyPiecesStuckCard.cs")

# For the rule:
# - Team should not matter
# - Type of piece should not matter
# 
# As long as a piece is next to another piece, it should be
# able to take actions.

func _verify_piece_movement(piece: Node, piece_name: String, should_have_valid: bool, additional: String = "") -> void:
	var not_text = " not"
	if should_have_valid:
		not_text = ""
	
	if piece_has_actions(piece, true) != should_have_valid:
		fail_test("%s should%s have valid actions %s" % [piece_name, not_text, additional])
	else:
		pass_test("%s should%s have valid actions %s" % [piece_name, not_text, additional])
	
	assert_ne(piece_has_tag(piece, "lonely_piece"), should_have_valid, "%s should%s have lonely_piece tag %s" % [piece_name, not_text, additional])

func test_card_team() -> void:
	var card = card_script.new()
	card.cardId = "lonely_pieces"
	
	# Place the pieces
	# 🐴  . 
	#  .     . 
	#  ♗ 🤴
	var bishop = place_piece("bishop", 0, 0, 0, 0)
	var knight = place_piece("knight", 0, 0, 0, 2)
	var queen = place_piece("queen", 0, 0, 1, 0)
	
	# Start the game with the card
	add_card(card)
	
	start_game()
	
	# The knight should have no movement options, but the pawn and queen
	# should.
	_verify_piece_movement(knight, "Knight", false, "(1)")
	_verify_piece_movement(bishop, "Bishop", true, "(1)")
	_verify_piece_movement(queen, "Queen", true, "(1)")
	
	# Move the bishop up one space.
	# 🐴  . 
	#  ♗   . 
	#  .   🤴
	move_piece(bishop, 0, 1)
	
	# Skip to the next turn
	next_turn(0)
	
	# Now only the queen is separated
	_verify_piece_movement(knight, "Knight", true, "(2)")
	_verify_piece_movement(bishop, "Bishop", true, "(2)")
	_verify_piece_movement(queen, "Queen", false, "(2)")
	
	# Now move the queen next to the bishop
	# 🐴  . 
	#  ♗ 🤴
	#  .     . 
	move_piece(queen, 1, 1)
	
	# Turn doesn't matter
	next_turn()
	
	# All pieces are next to another piece
	_verify_piece_movement(knight, "Knight", true, "(3)")
	_verify_piece_movement(bishop, "Bishop", true, "(3)")
	_verify_piece_movement(queen, "Queen", true, "(3)")
	
	# Move bishop down again, and none should be able to move
	# 🐴  . 
	#   .  🤴
	#  ♗  . 
	move_piece(bishop, 0, 0)
	
	# Turn doesn't matter
	next_turn()
	
	# No piece should be able to move
	_verify_piece_movement(knight, "Knight", false, "(4)")
	_verify_piece_movement(bishop, "Bishop", false, "(4)")
	_verify_piece_movement(queen, "Queen", false, "(4)")
	
	

func test_card_enemies() -> void:
	
	var card = card_script.new()
	card.cardId = "lonely_pieces"
	
	# Add two pieces on opposing teams
	#  .   🤴  .  
	# 🤴  .  🤴
	var queen = place_piece("queen", 0, 0, 0, 0)
	var e_queen = place_piece("queen", 0, 1, 1, 1)
	var e2_queen = place_piece("queen", 0, 2, 2, 0)
	
	# Start the game with the card
	game_controller.AddCard(card)
	start_game()
	
	# No queen should be able to move
	_verify_piece_movement(queen, "Queen 1", false, "(1)")
	_verify_piece_movement(e_queen, "Queen 2", false, "(1)")
	_verify_piece_movement(e2_queen, "Queen 3", false, "(1)")
	
	# Move Queen 1 up one space
	# 🤴🤴  .  
	#   .    .  🤴
	move_piece(queen, 0, 1)
	
	# Turn doesn't matter
	next_turn()
	
	# Queen 1 and 2 should be able to move
	_verify_piece_movement(queen, "Queen 1", true, "(2)")
	_verify_piece_movement(e_queen, "Queen 2", true, "(2)")
	_verify_piece_movement(e2_queen, "Queen 3", false, "(2)")
	
	# Move Queen 3 up one space
	# 🤴🤴🤴
	#   .    .    .  
	move_piece(e2_queen, 2, 1)
	
	# Go to next turn
	next_turn()
	
	# All queens should be able to move
	_verify_piece_movement(queen, "Queen 1", true, "(3)")
	_verify_piece_movement(e_queen, "Queen 2", true, "(3)")
	_verify_piece_movement(e2_queen, "Queen 3", true, "(3)")
	
	# Move Queen 2 down one space
	# 🤴  .  🤴
	#   .  🤴  .  
	move_piece(e_queen, 1, 0)
	
	# Go to next turn
	next_turn()
	
	# No queen should be able to move
	_verify_piece_movement(queen, "Queen 1", false, "(4)")
	_verify_piece_movement(e_queen, "Queen 2", false, "(4)")
	_verify_piece_movement(e2_queen, "Queen 3", false, "(4)")
