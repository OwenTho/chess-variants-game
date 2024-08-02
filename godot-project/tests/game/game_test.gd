extends GutTest

class_name GameTest

var game_controller_script: CSharpScript = preload("res://scripts/game/GameController.cs")
var game_controller: Object
var game_state: Object

func before_all() -> void:
	# Make a new GameController
	game_controller = game_controller_script.new()
	add_child(game_controller)
	
	# Disable the thread, so that any following code only processes
	# after the game is done processing
	game_controller.singleThread = true

# Before each test, initialise the board
func before_each() -> void:
	# And then initialise a new game
	game_controller.FullInit(true)
	
	# Store the GameController variables so that they're easy to access
	game_state = game_controller.currentGameState

func after_each() -> void:
	# Free the game state
	game_state.free()


func print_current_board() -> void:
	# Get all of the cell positions
	var min_pos = Vector2i(0,0)
	var max_pos = Vector2i(7,7)
	for piece in game_state.allPieces:
		var cell = piece.cell
		if min_pos == null:
			min_pos = cell.pos
		if max_pos == null:
			max_pos = cell.pos
		
		if cell.x > max_pos.x:
			max_pos.x = cell.x
		if cell.y > max_pos.y:
			max_pos.y = cell.y
		if cell.x < min_pos.x:
			min_pos.x = cell.x
		if cell.y < min_pos.y:
			min_pos.y = cell.y
	
	var total_pos: Vector2i = max_pos - min_pos
	var piece_grid: Array[Array] = []
	for y in range(total_pos.y + 1):
		var cur_arr: Array[String] = []
		piece_grid.append(cur_arr)
		for x in range(total_pos.x + 1):
			# Get all first Piece at this position
			var piece = game_state.GetFirstPieceAt(min_pos.x + x, min_pos.y + y)
			if piece == null:
				cur_arr.append(". ")
			elif piece.info == null:
				cur_arr.append("X")
			else:
				cur_arr.append(get_piece_string(piece.info.pieceId))
	
	
	print(total_pos)
	
	# Print each individual line
	for i in range(total_pos.y, -1, -1):
		var cur_line: Array[String] = piece_grid[i]
		print("".join(cur_line))

func get_piece_string(piece_id: String):
	match piece_id:
		"king":
			return "👑"
		"queen":
			return "🤴"
		"rook":
			return "🏰"
		"knight":
			return "🐴"
		"bishop":
			return "✝️"
		"pawn":
			return "♟️"
		_:
			return "❓"
