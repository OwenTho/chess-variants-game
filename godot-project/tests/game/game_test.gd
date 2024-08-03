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


func after_all() -> void:
	# Free the game controller
	game_controller.free()




func piece_has_actions_at(piece, location: Vector2i, num_ignored: int = 0) -> bool:
	var actions_at_pos: int = 0
	for cell in game_state.actionGrid.cells:
		if cell.pos == location:
			for action in cell.items:
				if piece.currentPossibleActions.has(action):
					actions_at_pos += 1
					if actions_at_pos > num_ignored:
						return true
	
	return false





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
				# If no pieces, check if there's an action
				var skip_empty := false
				var is_acting := false
				var is_valid := false
				for cell in game_state.actionGrid.cells:
					# Find at least one active / acting action
					for action in cell.items:
						if action.valid:
							is_valid = true
						if action.acting:
							is_acting = true
					if cell.pos.x == x and cell.pos.y == y:
						if is_valid:
							cur_arr.append("  @  ")
						elif is_acting:
							cur_arr.append("  #  ")
						skip_empty = true
						break
				if not skip_empty:
					cur_arr.append("  .  ")
			elif piece.info == null:
				cur_arr.append("X")
			else:
				cur_arr.append(get_piece_string(piece.info.pieceId))
	
	var player_status: Array[String] = []
	
	for player_num in game_controller.NUMBER_OF_PLAYERS:
		player_status.append("Ok")
		if game_state.PlayerInCheck(player_num):
			player_status[player_num] = "Check"
		elif game_state.PlayerHasNoKing(player_num):
			player_status[player_num] = "No King"
	
	print("Player turn: %s" % [game_state.currentPlayerNum])
	print("P1 status: %s" % [player_status[0]])
	print("P2 status: %s" % [player_status[1]])
	
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
