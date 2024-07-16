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
	place_piece("pawn", 0, 0, 1)
	place_piece("pawn", 0, 1, 1)
	place_piece("pawn", 0, 2, 1)
	place_piece("pawn", 0, 3, 1)
	place_piece("pawn", 0, 4, 1)
	place_piece("pawn", 0, 5, 1)
	place_piece("pawn", 0, 6, 1)
	place_piece("pawn", 0, 7, 1)
	place_piece("rook", 0, 0, 0)
	place_piece("knight", 0, 1, 0)
	place_piece("bishop", 0, 2, 0)
	place_piece("queen", 0, 3, 0)
	place_piece("king", 0, 4, 0)
	place_piece("bishop", 0, 5, 0)
	place_piece("knight", 0, 6, 0)
	place_piece("rook", 0, 7, 0)

func place_piece(piece_id: String, id: int, x: int, y: int) -> void:
	var info = game_controller.GetPieceInfo(piece_id)
	if info == null:
		push_error("Tried to place piece '%s', but the piece isn't registered." % [piece_id])
		return
	# Make a new piece
	var new_piece: Piece2D = piece_scene.instantiate()
	new_piece.board = board
	
	# Add node to tree
	get_tree().get_first_node_in_group("piece_holder").add_child(new_piece)
	
	new_piece.piece_data.info = info
	new_piece.piece_data.pieceId = id
	game_controller.grid.PlaceItemAt(new_piece.piece_data, x, y)
	var image_loc: String = "assets/texture/piece/" + info.textureLoc
	var piece_sprite: Texture
	if FileAccess.file_exists("res://" + image_loc):
		piece_sprite = load("res://" + image_loc)
	else:
		push_warning("Could not find sprite at path '%s', so defalt is being used." % [image_loc])
		piece_sprite = load("res://assets/texture/piece/default.png")
	
	new_piece.set_sprite(piece_sprite)
	
	print("Placed %s (id: %s) at %s,%s" % [piece_id, id, x, y])
