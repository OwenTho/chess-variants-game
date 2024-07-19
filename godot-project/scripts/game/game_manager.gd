extends Node

var game_controller_script: CSharpScript = preload("res://scripts/game/GameController.cs")
var game_controller: Object

var game_scene: PackedScene = preload("res://scenes/game/game_screen.tscn")
var piece_scene: PackedScene = preload("res://scenes/game/piece.tscn")

var game: Game
var grid
var board: Board2D

signal has_init()

func _ready():
	Lobby.server_disconnected.connect(_on_server_disconnect)

func _on_server_disconnect():
	reset_game()
	#get_tree().change_scene_to_file("res://scenes/menu/main_menu.tscn")

func reset_game():
	if game != null:
		game.close_game()
	board = null
	game = null
	# game.queue_free()
	grid = null

func init() -> void:
	if game != null:
		game.queue_free()
	
	var cur_scene = get_tree().current_scene
	game = game_scene.instantiate()
	get_tree().root.add_child(game)
	get_tree().current_scene = game
	cur_scene.queue_free()
	
	board = game.board
	
	# Initialise the game controller
	game_controller = game_controller_script.new()
	
	setup_signals()
	
	# Initialise the game
	game_controller.FullInit()
	grid = game_controller.grid
	
	has_init.emit()

func setup_signals():
	game_controller.NewTurn.connect(game._on_next_turn)
	game_controller.EndTurn.connect(game._on_end_turn)
	game_controller.RequestedActionAt.connect(game._on_requested_action)

func setup_game(new_board: Board2D) -> void:
	board = new_board
	
	# First, make a new GameController
	game_controller = game_controller_script.new()
	
	# Initialise the game
	game_controller.FullInit()
	self.grid = game_controller.grid
	
	# Load the game scene
	init_board()
	

func start_game():
	game_controller.StartGame()

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

func board_to_array() -> Array:
	var ret_array: Array = []
	for cell in grid.cells:
		var cell_pos: Vector2i = cell.pos
		for item in cell.items:
			var this_item: Array = []
			# TODO: Update GridItem to have its own ID, which
			# id is saved to instead of pieceId for Pieces. This would be
			# for custom spaces.
			# This works for now, however, as there are only pieces.
			# Add the ID first
			this_item.append(item.info.pieceId)
			# Then add the link ID
			this_item.append(item.linkId)
			# The team of the piece
			this_item.append(item.teamId)
			# The position of the item
			this_item.append(cell_pos)
			# The id of the item
			this_item.append(item.id)
			
			ret_array.append(this_item)
	return ret_array

func load_board(board_data: Array):
	for item in board_data:
		place_piece(item[0], item[1], item[2], item[3].x, item[3].y, item[4])

func place_piece(piece_id: String, link_id: int, team: int, x: int, y: int, id: int = -1) -> bool:
	# Request the game controller to make the piece
	var new_piece_data = game_controller.PlacePiece(piece_id, link_id, team, x, y, id)
	
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




