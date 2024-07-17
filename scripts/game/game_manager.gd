extends Node

var game_controller_script: CSharpScript = preload("res://scripts/game/GameController.cs")
var game_controller: Object

var game_screen: PackedScene = preload("res://scenes/game/game_screen.tscn")
var piece_scene: PackedScene = preload("res://scenes/game/piece.tscn")

var grid
var board: Board2D

func start(new_board: Board2D) -> void:
	board = new_board
	
	# First, make a new GameController
	game_controller = game_controller_script.new()
	
	# Initialise the game
	game_controller.FullInit()
	self.grid = game_controller.grid
	
	# Load the game scene
	init_board()

func init_board() -> void:
	# Add all of the pieces
	place_matching("pawn", 0, 0, 1)
	place_matching("pawn", 1, 1, 1)
	place_matching("pawn", 2, 2, 1)
	place_matching("pawn", 3, 3, 1)
	place_matching("pawn", 4, 4, 1)
	place_matching("pawn", 5, 5, 1)
	place_matching("pawn", 6, 6, 1)
	place_matching("pawn", 7, 7, 1)
	place_matching("rook", 8, 0, 0)
	place_matching("knight", 9, 1, 0)
	place_matching("bishop", 10, 2, 0)
	place_matching("queen", 11, 3, 0)
	place_matching("king", 12, 4, 0)
	place_matching("bishop", 13, 5, 0)
	place_matching("knight", 14, 6, 0)
	place_matching("rook", 15, 7, 0)

func place_piece(piece_id: String, id: int, team: int, x: int, y: int) -> bool:
	# Request the game controller to make the piece
	var new_piece_data = game_controller.PlacePiece(piece_id, id, team, x, y)
	
	# If it failed to place the piece, return false
	if new_piece_data == null:
		return false
	
	# Now that we have a new piece, initialise the Piece2D
	var new_piece: Piece2D = piece_scene.instantiate()
	
	# Add the piece data
	new_piece.piece_data = new_piece_data
	new_piece.board = board
	
	new_piece.add_child(new_piece_data)
	
	# Update the position and sprite
	new_piece.update_pos()
	new_piece.update_sprite()
	
	# Add node to tree
	get_tree().get_first_node_in_group("piece_holder").add_child(new_piece)
	
	print("Placed %s (id: %s) at %s,%s" % [piece_id, id, x, y])
	return true

func place_matching(piece_id: String, id: int, x: int, y: int) -> void:
	place_piece(piece_id, id, 0, x, y)
	place_piece(piece_id, id, 1, x, game_controller.gridSize.y - y - 1)
