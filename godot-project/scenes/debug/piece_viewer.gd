extends Node2D

@export var board: Board2D

@export var action_highlights: Node2D
@export var highlight_scene: PackedScene

var game_active: bool = false

var cursor_over_game: bool = false
var disabled_selection: bool = false

var allow_quit: bool = true

var game_controller: GameController

var rotating_board: bool = false
var rotate_start: Vector2

var updating: bool = false
var piece: Piece2D
var piece_ids: Array
var cur_piece_id_ind: int

func _ready() -> void:
	updating = true
	# Create the GameController
	game_controller = GameController.new()
	add_child(game_controller)
	# Connect the signals
	game_controller.TurnStarted.connect(_on_turn_started)
	# Full Init
	game_controller.FullInit(true, 1)
	# Wait a frame
	await get_tree().process_frame
	# Get the piece ids
	piece_ids = game_controller.GetPieceKeys()
	# Make the grid much bigger
	game_controller.currentGameState.gridUpperCorner = Vector2i(32,32)
	game_controller.currentGameState.gridLowerCorner = Vector2i(-32,-32)
	
	# Add a piece to 0, 0 that uses the first piece id.
	const piece_scene: PackedScene = preload("res://scenes/game/piece/piece.tscn")
	piece = piece_scene.instantiate()
	piece.piece_data = game_controller.PlacePiece(piece_ids[0], 0, 0, 0, 0, -1)
	piece.board = board
	
	piece.update_pos()
	piece.update_sprite()
	
	# Add node to tree
	get_tree().get_first_node_in_group("piece_holder").add_child(piece)
	
	cur_piece_id_ind = 0
	
	updating = true
	game_controller.NextTurn()

func set_to_piece_id(index: int) -> void:
	updating = true
	# If it's < 0, add until it's above
	while index < 0:
		index += piece_ids.size()
	# If it's too high, mod it down
	index %= piece_ids.size()
	
	# Default times moved to 0 for new pieces
	piece.piece_data.timesMoved = 0
	# Default level to 0 for new pieces
	piece.piece_data.level = 0
	
	var piece_id = piece_ids[index]
	var piece_info: PieceInfo = game_controller.GetPieceInfo(piece_id)
	piece.piece_data.info = piece_info
	
	print("Showing piece %s (%s)" % [piece_id, index])
	cur_piece_id_ind = index

	# Now that the info is updated, start next turn
	piece.piece_data.EnableActionsUpdate()
	game_controller.NextTurn()

func set_piece_times_moved(times_moved: int) -> void:
	updating = true
	piece.piece_data.timesMoved = times_moved
	print("Times moved updated to %s" % [piece.piece_data.timesMoved])
	
	piece.piece_data.EnableActionsUpdate()
	game_controller.NextTurn()

func set_piece_level(level: int) -> void:
	updating = true
	piece.piece_data.level = level
	print("Level updated to %s" % [piece.piece_data.level])
	
	piece.piece_data.EnableActionsUpdate()
	game_controller.NextTurn()

func _input(event: InputEvent) -> void:
	if not updating:
		viewer_inputs(event)

func viewer_inputs(event: InputEvent) -> void:
	updating = true
	if event.is_action_pressed("viewer_next_piece"):
		set_to_piece_id(cur_piece_id_ind + 1)
		return
	if event.is_action_pressed("viewer_prev_piece"):
		set_to_piece_id(cur_piece_id_ind - 1)
		return
	if event.is_action_pressed("viewer_increase_times_moved"):
		set_piece_times_moved(piece.piece_data.timesMoved + 1)
		return
	if event.is_action_pressed("viewer_decrease_times_moved"):
		set_piece_times_moved(piece.piece_data.timesMoved - 1)
		return
	if event.is_action_pressed("viewer_level_up"):
		set_piece_level(piece.piece_data.level + 1)
		return
	if event.is_action_pressed("viewer_level_down"):
		set_piece_level(piece.piece_data.level - 1)
		return
	updating = false

func actions_updated() -> void:
	# Update the piece's action locations
	piece.set_actions(GameManager.get_piece_actions(piece.piece_data))
	# Update the selection
	update_selection()
	# Wait for the timer to finish, to give a slight delay to avoid actions
	# being updated too quickly
	$UpdateTimer.stop()
	$UpdateTimer.start()
	await $UpdateTimer.timeout
	updating = false

func _on_turn_started(player_num: int) -> void:
	actions_updated()

func delete_selection() -> void:
	for child in action_highlights.get_children():
		child.queue_free()

func update_selection() -> void:
	delete_selection()
	
	# If there is no selected piece, then ignore it
	if piece == null:
		return
	
	var possible_actions: Array[Vector2i] = piece.possible_actions
	# If the piece has no actions, then ignore it
	if possible_actions == null:
		return
	
	for action in possible_actions:
		var new_highlight: Node2D = highlight_scene.instantiate()
		
		new_highlight.board = board
		new_highlight.scale = Vector2(.85, .85)
		new_highlight.set_pos(action.x, action.y)
		action_highlights.add_child.call_deferred(new_highlight)





# Board size
func _pos_to_cell_pos(pos: Vector2i) -> Vector2i:
	pos -= Vector2i(4,3)
	pos *= Vector2i(1,-1)
	return pos

func _add_cell_at(pos: Vector2i) -> void:
	pos = _pos_to_cell_pos(pos)
	var tile_pos: Vector2i = Vector2i(0,0)
	if (abs(pos.x) % 2) == (abs(pos.y) % 2):
		tile_pos = Vector2i(1,0)
	
	%AdditionalTiles.set_cell(pos, 0, tile_pos)

func _remove_cell_at(pos: Vector2i) -> void:
	%AdditionalTiles.set_cell(_pos_to_cell_pos(pos))

func _add_cells_to_range(pos1: Vector2i, pos2: Vector2i) -> void:
	if pos2.x < pos1.x:
		var temp = pos1.x
		pos1.x = pos2.x
		pos2.x = temp
	if pos2.y < pos1.y:
		var temp = pos1.y
		pos1.y = pos2.y
		pos2.y = temp
	
	for x in range(pos1.x, pos2.x+1):
		for y in range(pos1.y, pos2.y+1):
			_add_cell_at(Vector2i(x, y))
	
func _remove_cells_in_range(pos1: Vector2i, pos2: Vector2i) -> void:
	if pos2.x < pos1.x:
		var temp = pos1.x
		pos1.x = pos2.x
		pos2.x = temp
	if pos2.y < pos1.y:
		var temp = pos1.y
		pos1.y = pos2.y
		pos2.y = temp
	
	for x in range(pos1.x, pos2.x+1):
		for y in range(pos1.y, pos2.y+1):
			_remove_cell_at(Vector2i(x, y))


func _on_grid_size_changed(old_lower_bound: Vector2i, old_upper_bound: Vector2i, new_lower_bound: Vector2i, new_upper_bound: Vector2i) -> void:
	#        x
	#  ------x
	#  |    |x
	#  |    |x
	#  ------x
	#        x
	if new_upper_bound.x < old_upper_bound.x:
		_remove_cells_in_range(Vector2i(new_upper_bound.x+1, old_lower_bound.y), Vector2i(old_upper_bound.x, old_upper_bound.y))
	if new_upper_bound.x > old_upper_bound.x:
		_add_cells_to_range(Vector2i(old_upper_bound.x+1, new_lower_bound.y), Vector2i(new_upper_bound.x, new_upper_bound.y))
	# xxxxxxxx
	#  ------
	#  |    |
	#  |    |
	#  ------
	#       
	if new_upper_bound.y < old_upper_bound.y:
		_remove_cells_in_range(Vector2i(new_lower_bound.x, new_upper_bound.y+1), Vector2i(old_upper_bound.x, old_upper_bound.y))
	if new_upper_bound.y > old_upper_bound.y:
		_add_cells_to_range(Vector2i(new_lower_bound.x, old_upper_bound.y+1), Vector2i(new_upper_bound.x, new_upper_bound.y))
	## LOWER BOUND
	# x
	# x------
	# x|    |
	# x|    |
	# x------
	# x
	if new_lower_bound.x > old_lower_bound.x:
		_remove_cells_in_range(Vector2i(old_lower_bound.x, new_lower_bound.y), Vector2i(new_lower_bound.x-1, old_upper_bound.y))
	if new_lower_bound.x < old_lower_bound.x:
		_add_cells_to_range(Vector2i(new_lower_bound.x, new_lower_bound.y), Vector2i(old_lower_bound.x-1, new_upper_bound.y))
	#  
	#  ------
	#  |    |
	#  |    |
	#  ------
	# xxxxxxxx
	if new_lower_bound.x > old_lower_bound.x:
		_remove_cells_in_range(old_lower_bound, Vector2i(new_upper_bound.x, new_lower_bound.y-1))
	if new_lower_bound.x < old_lower_bound.x:
		_add_cells_to_range(new_lower_bound, Vector2i(new_upper_bound.x, old_lower_bound.y-1))


func _end_game_message(title: String, message: String) -> void:
	var pop_up: AcceptDialog = AcceptDialog.new()
	pop_up.title = title
	pop_up.dialog_text = message
	pop_up.dialog_hide_on_ok = false
	add_child(pop_up)
	pop_up.close_requested.connect(to_lobby)
	pop_up.close_requested.connect(pop_up.queue_free)
	pop_up.confirmed.connect(to_lobby)
	pop_up.confirmed.connect(pop_up.queue_free)
	
	
	pop_up.popup_centered()
	
	# Disable board rotation
	rotating_board = false

func _on_player_resigned(player_num: int) -> void:
	_end_game_message("Resigned", "Player %s has resigned, letting player %s win." % [player_num+1, 2-player_num])

func _on_player_won(winner: int) -> void:
	_end_game_message("Checkmate!", "Player %s has won." % [winner])

func _on_player_lost(player_num: int) -> void:
	pass

func _on_game_stalemate(stalemate_player: int) -> void:
	_end_game_message("Stalemate.", "Player %s is unable to act, putting the game in Stalemate." % [stalemate_player])



func to_lobby() -> void:
	GameManager.reset_game()
	if not multiplayer.has_multiplayer_peer() or multiplayer.multiplayer_peer is OfflineMultiplayerPeer:
		Lobby.remove_multiplayer_peer()
		get_tree().change_scene_to_file("res://scenes/menu/main_menu.tscn")
	else:
		get_tree().change_scene_to_file("res://scenes/menu/lobby_menu.tscn")

func to_menu() -> void:
	get_tree().change_scene_to_file("res://scenes/menu/main_menu.tscn")




func _on_game_input_mouse_entered() -> void:
	cursor_over_game = true


func _on_game_input_mouse_exited() -> void:
	cursor_over_game = false

func _process(delta: float) -> void:
	if rotating_board:
		# Get current mouse position
		var cur_mouse_position: Vector2 = get_global_mouse_position()
		# Get the angle of the two
		var angle_from = $BoardHolder.global_position.angle_to_point(rotate_start)
		var angle_to = $BoardHolder.global_position.angle_to_point(cur_mouse_position)
		# Rotate so angle_from is 0
		angle_to -= angle_from
		angle_from -= angle_from
		# Lerp the angle towards
		var rotate_angle = lerp_angle(angle_from, angle_to, 1 - pow(0.2, delta))
		# Clamp, so that it can't spin too quickly
		rotate_angle = clampf(rotate_angle, deg_to_rad(-45), deg_to_rad(45))
		# Update the rotation, and lerp rotate_start
		rotate_start = lerp(rotate_start, cur_mouse_position, 1 - pow(0.2, delta))
		$BoardHolder.rotation = $BoardHolder.rotation + rotate_angle


func _on_game_input_gui_input(event: InputEvent) -> void:
	if event.is_action_pressed("mouse_right"):
		rotating_board = true
		rotate_start = get_global_mouse_position()
		return
	elif event.is_action_released("mouse_right"):
		rotating_board = false
		return
