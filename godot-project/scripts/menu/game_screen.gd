extends Node2D

class_name Game

@export var board: Board2D
@export var cursor: BoardItem2D

@export var action_highlights: Node2D
@export var highlight_scene: PackedScene

var selected_piece: Piece2D
var possible_actions: Array

func _ready() -> void:
	GameManager.has_init.connect(_on_init)
	var my_num = Lobby.get_player_num(multiplayer.get_unique_id())
	# If it's player 2, flip the board
	if my_num == 1:
		$BoardHolder.rotation = deg_to_rad(180)

func _on_init():
	cursor.board = GameManager.board
	
	Debug.stats.add_property(GameManager.game_controller, "currentPlayerNum")

func _process(delta) -> void:
	if Input.is_action_pressed("mouse_right"):
		$BoardHolder.rotation = $BoardHolder.rotation + deg_to_rad(45) * delta

func _input(event) -> void:
	if event is InputEventMouseButton and event.is_action_pressed("mouse_left"):
		
		# Get the piece the player is selecting
		var cell_pos: Vector2i = cursor.last_cell
		
		select_cell(cell_pos)

func remove_selection() -> void:
	selected_piece = null
	possible_actions = []
	for child in action_highlights.get_children():
		child.queue_free()

func select_cell(cell_pos: Vector2i):
	# First check if the player is selecting an action
	var actions_to_take: Array = []
	for action in possible_actions:
		if action.actionLocation == cell_pos:
			actions_to_take.append(action)
	
	if actions_to_take.size() > 0:
		var acted: bool = false
		GameManager.game_controller.RequestActionsAt(cell_pos, selected_piece.piece_data)
		return
	
	# If not any of the above, check if there is a cell on the Grid
	var cell = GameManager.grid.GetCellAt(cell_pos.x, cell_pos.y)
	
	# Remove existing selection before moving on	
	remove_selection()
	
	# Get the first piece
	var piece = GameManager.game_controller.GetFirstPieceAt(cell_pos.x, cell_pos.y)
	if piece == null:
		return
	var item_node = piece.get_parent()
	
	select_item(item_node)

func select_item(piece: Piece2D) -> void:
	selected_piece = piece
	if piece.piece_data == null or piece.piece_data.info == null:
		return
	possible_actions = piece.piece_data.currentPossibleActions
	if possible_actions == null:
		return
	
	for action in possible_actions:
		# Skip if invalid
		if not action.valid:
			continue
		var new_highlight: Node2D = highlight_scene.instantiate()
		
		new_highlight.board = board
		new_highlight.set_pos(action.actionLocation.x, action.actionLocation.y)
		action_highlights.add_child.call_deferred(new_highlight)

@rpc("any_peer", "call_remote", "reliable")
func next_turn(new_player_num: int) -> void:
	GameManager.game_controller.NextTurn(new_player_num)
	remove_selection()

func _on_next_turn(new_player_num: int):
	remove_selection()
	next_turn.rpc(new_player_num)

func _on_end_turn() -> void:
	remove_selection()

@rpc("authority", "call_remote", "reliable")
func take_action_at(piece_id: int, action_location: Vector2i):
	var piece = GameManager.game_controller.GetPiece(piece_id)
	if piece != null:
		GameManager.game_controller.TakeActionAt(action_location, piece)

@rpc("any_peer", "call_local", "reliable")
func request_action(piece_id: int, action_location: Vector2i) -> void:
	# Ignore if not authority
	if not is_multiplayer_authority():
		return
	
	# Get the player number of this player
	var player_num: int = Lobby.get_player_num(multiplayer.get_remote_sender_id())
	# If it's -1, ignore
	if (player_num == -1):
		return
	
	# If it's the wrong player number, ignore
	var cur_player_num = GameManager.game_controller.currentPlayerNum
	if Lobby.player_nums[cur_player_num] != multiplayer.get_remote_sender_id():
		return
	
	# Get piece
	var piece = GameManager.game_controller.GetPiece(piece_id)
	
	# If piece id is invalid, ignore
	if piece == null:
		return
	
	# Verify actions at that location
	var valid_actions: bool = true
	var actions: Array = piece.GetPossibleActions(GameManager.game_controller)
	
	# If there are no actions, ignore
		return
	
	# Take the actions
	# Tell everyone to take the action
	take_action_at.rpc(piece_id, action_location)
	
	# Go to the next turn
	GameManager.game_controller.NextTurn()

func _on_requested_action(piece_id: int, action_location: Vector2i) -> void:
	request_action.rpc(piece_id, action_location)
func _on_requested_action(action_location: Vector2i, piece) -> void:
	request_action.rpc(piece.id, action_location)

func close_game():
	
	get_tree().change_scene_to_file("res://scenes/menu/main_menu.tscn")

func _on_btn_quit_pressed():
	# Disconnect GameManager signal to avoid breaking
	Lobby.server_disconnected.disconnect(GameManager._on_server_disconnect)
	# Close the game
	multiplayer.multiplayer_peer.close()
	# Call the GameManager disconnect function (since we stopped it being called)
	GameManager._on_server_disconnect()
	# Reconnect the signal
	Lobby.server_disconnected.connect(GameManager._on_server_disconnect)
