extends GutTest

var card_script: CSharpScript = preload("res://scripts/game/card/major/FriendlyFireCard.cs")
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
	card.CardId = "friendly_fire"
	
	# Make the board
	#  ♟️  🤴
	var pawn = game.place_piece("pawn", 0, 0, 0, 0)
	var queen = game.place_piece("queen", 0, 0, 1, 0)
	
	game.start_game()
	
	# Initially, queen should not be able to take the pawn
	assert_false(game.piece_act_at(queen, 0, 0), "The Queen should not be able to act on the pawn's space.")
	assert_true(game.piece_on_cell(queen, 1, 0), "The Queen should not have moved.")
	assert_true(game.piece_on_board(pawn), "The Pawn should still be on the board")
	
	# Now add the card
	game.add_card(card)
	
	# Skip to player's next turn
	game.next_turn(0)
	
	# Queen should now be able to take the pawn
	assert_true(game.piece_act_at(queen, 0, 0), "The Queen should be able to act on the pawn's space.")
	assert_true(game.piece_on_cell(queen, 0, 0), "The Queen should have moved to the Pawn's cell.")
	assert_false(game.piece_on_board(pawn), "The Pawn should no longer be on the board")
