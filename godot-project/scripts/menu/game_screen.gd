extends Node2D

class_name Game

@export var board: Board2D
@export var cursor: BoardItem2D

@export var action_highlights: Node2D
@export var highlight_scene: PackedScene

@export var notices: VBoxContainer

var selected_piece: Piece2D

var game_active: bool = false
var disabled_selection: bool = false

var allow_quit: bool = true

func _ready() -> void:
	GameManager.has_init.connect(_on_init)
	GameManager.next_turn.connect(_on_next_turn)
	GameManager.end_turn.connect(_on_end_turn)
	
	GameManager.taking_action_at.connect(_on_taking_action)
	GameManager.action_was_failed.connect(_on_action_failed)
	
	GameManager.player_has_won.connect(_on_player_won)
	GameManager.player_lost.connect(_on_player_lost)
	GameManager.piece_taken.connect(_on_piece_taken)
	
	GameManager.notice_received.connect(_on_notice_received)
	
	var my_num = Lobby.get_player_num(multiplayer.get_unique_id())
	# If it's player 2, flip the board
	if my_num == 1:
		$BoardHolder.rotation = deg_to_rad(180)
	
	# If the game is not opened by a player, hide the quit button
	if not Lobby.is_player:
		$GameUI/BtnQuit.visible = false

func _on_init() -> void:
	cursor.board = GameManager.board
	
	Debug.stats.add_property(GameManager.game_controller.currentGameState, "currentPlayerNum")

func _process(delta) -> void:
	if Input.is_action_pressed("mouse_right"):
		$BoardHolder.rotation = $BoardHolder.rotation + deg_to_rad(45) * delta

func _on_cursor_highlight_cell_updated(new_cell: Vector2i) -> void:
	# If cursor is outside range, then hide it
	cursor.visible = (GameManager.spaces_off_board(cursor.last_cell.x, cursor.last_cell.y) == 0) and cursor.active

func _input(event) -> void:
	# Ignore inputs from non-players
	if not Lobby.is_player:
		return
	if not game_active:
		return
	if event is InputEventMouseButton and event.is_action_pressed("mouse_left"):
		# Get the piece the player is selecting
		var cell_pos: Vector2i = cursor.last_cell
		
		if disabled_selection:
			return
		select_cell(cell_pos)

func remove_selection() -> void:
	selected_piece = null
	for child in action_highlights.get_children():
		child.queue_free()

func select_cell(cell_pos: Vector2i) -> void:
	# Try to get the game mutex
	if not GameManager.game_mutex.try_lock():
		# If it couldn't get the mutex, then return
		return
	# First check if the player is selecting an action
	var actions_to_take: Array = []
	if selected_piece != null and selected_piece.piece_data != null and selected_piece.piece_data.currentPossibleActions != null: 
		# Check if there is an action being selected
		for action in selected_piece.piece_data.currentPossibleActions:
			if action.actionLocation == cell_pos and action.valid and action.acting:
				# If there is, request to act on that location
				GameManager.game_mutex.unlock()
				disabled_selection = true
				allow_quit = false
				GameManager.request_action.rpc(cell_pos, selected_piece.piece_data.id)
				remove_selection()
				return
	# Remove lock
	GameManager.game_mutex.unlock()
	
	# Remove existing selection before moving on	
	remove_selection()
	
	# Get the first piece
	var piece = await GameManager.get_first_piece_at(cell_pos.x, cell_pos.y)
	if piece == null:
		return
	
	var item_node: Piece2D = GameManager.get_piece_id(piece.id)
	
	select_item(item_node)

func select_item(piece: Piece2D) -> void:
	selected_piece = piece
	# If a piece is missing data, or missing info, then it can't
	# be played
	if piece == null or piece.piece_data == null:
		return
	if piece.piece_data.info == null:
		return
	
	# If the selection can't get the game mutex, just ignore
	if not GameManager.game_mutex.try_lock():
		return
	var possible_actions: Array = piece.piece_data.currentPossibleActions
	
	# If the piece has no actions, then ignore it
	if possible_actions == null:
		return
	
	var checked_locations: Array[Vector2i] = []
	for action in possible_actions:
		# Skip if invalid or not an acting action
		if not action.valid or not action.acting:
			continue
		# Ignore if position already checked
		if checked_locations.has(action.actionLocation):
			continue
		checked_locations.append(action.actionLocation)
		var new_highlight: Node2D = highlight_scene.instantiate()
		
		new_highlight.board = board
		new_highlight.set_pos(action.actionLocation.x, action.actionLocation.y)
		action_highlights.add_child.call_deferred(new_highlight)
	# Unlock the mutex once done
	GameManager.game_mutex.unlock()





func _on_next_turn(new_player_num: int) -> void:
	remove_selection()
	disabled_selection = false
	allow_quit = true

func _on_end_turn() -> void:
	pass

func _on_taking_action(action_location: Vector2i, piece) -> void:
	allow_quit = false

func _on_action_processed(success: bool, action_location: Vector2i, piece) -> void:
	if not is_multiplayer_authority():
		allow_quit = true
		return
	if not success:
		allow_quit = true
		return

func _on_action_failed(reason: String) -> void:
	if not reason.is_empty():
		notices.add_notice(reason)
	remove_selection()
	disabled_selection = false
	allow_quit = true

func _on_piece_taken(taken_piece, attacker) -> void:
	var remove_piece: Piece2D = GameManager.get_piece_id(taken_piece.id)
	if remove_piece == null:
		return
	remove_piece.queue_free()


func _on_player_won(winner: int) -> void:
	var pop_up: AcceptDialog = AcceptDialog.new()
	pop_up.dialog_text = "Player %s has won." % [winner]
	pop_up.title = "Checkmate!"
	pop_up.dialog_hide_on_ok = false
	add_child(pop_up)
	pop_up.close_requested.connect(to_lobby)
	pop_up.close_requested.connect(pop_up.queue_free)
	pop_up.confirmed.connect(to_lobby)
	pop_up.confirmed.connect(pop_up.queue_free)
	
	pop_up.popup_centered()

func _on_player_lost(player_num: int) -> void:
	pass

func _on_notice_received(text: String) -> void:
	notices.add_notice(text)



func to_lobby() -> void:
	GameManager.reset_game()
	if multiplayer.multiplayer_peer is OfflineMultiplayerPeer:
		get_tree().change_scene_to_file("res://scenes/menu/main_menu.tscn")
	else:
		get_tree().change_scene_to_file("res://scenes/menu/lobby_menu.tscn")

func to_menu() -> void:
	get_tree().change_scene_to_file("res://scenes/menu/main_menu.tscn")

func _on_btn_quit_pressed() -> void:
	if not allow_quit:
		notices.add_notice("Can't quit right now.")
		return
	game_active = false
	# Disconnect GameManager signal to avoid breaking
	Lobby.server_disconnected.disconnect(GameManager._on_server_disconnect)
	# Close the game
	if multiplayer.multiplayer_peer != null:
		multiplayer.multiplayer_peer.close()
	# Call the GameManager disconnect function (since we stopped it being called)
	GameManager._on_server_disconnect()
	# Reconnect the signal
	Lobby.server_disconnected.connect(GameManager._on_server_disconnect)
