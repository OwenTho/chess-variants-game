extends Node

var game_controller_script: CSharpScript = preload("res://scripts/game/GameController.cs")
var game_controller: Object

var board_scene: PackedScene = preload("res://scenes/game/board.tscn")
var piece_scene: PackedScene = preload("res://scenes/game/piece.tscn")

func start() -> void:
	# First, make a new GameController
	game_controller = game_controller_script.new()
	
	# Initialise the game
	game_controller.FullInit()
	
	# Load the game scene
	init_board()

func init_board() -> void:
	# Add all of the pieces
	place_piece("pawn", 0, -3, 0)
	place_piece("knight", 1, -2, 0)
	place_piece("bishop", 2, -1, 0)
	place_piece("bad_id", -1, 0, 0)
	place_piece("knight", 3, 1, 0)
	place_piece("queen", 4, 2, 0)
	place_piece("king", 4, 3, 0)

func place_piece(piece_id: String, id: int, x: int, y: int) -> void:
	var info = game_controller.GetPieceInfo(piece_id)
	if info == null:
		push_error("Tried to place piece '%s', but the piece isn't registered." % [piece_id])
		return
	# Make a new piece
	var new_piece: Node2D = piece_scene.instantiate()
	
	# Add node to tree
	
	get_tree().get_first_node_in_group("piece_holder").add_child(new_piece)
	
	new_piece.info = info
	new_piece.pieceId = id
	game_controller.grid.PlaceItemAt(new_piece, x, y)
	new_piece.position = Vector2(x * 128, y * 128)
	var image_loc: String = "assets/texture/piece/" + info.textureLoc
	#var piece_sprite: Texture2D = ImageTexture.create_from_image(Image.load_from_file(image_path))
	var piece_sprite: Texture
	if FileAccess.file_exists(image_loc):
		piece_sprite = load("res://" + image_loc)
	else:
		push_warning("Could not find sprite at path '%s', so defalt is being used." % [image_loc])
		piece_sprite = load("res://assets/texture/piece/default.png")
	new_piece.get_node("SprPiece").texture = piece_sprite
	
	print("Placed %s (id: %s) at %s,%s" % [piece_id, id, x, y])
