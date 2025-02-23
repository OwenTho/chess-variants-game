extends Node2D

class_name Game

@export var board: Board2D
@export var cursor: BoardItem2D

@export var card_selection: Control

@export var action_highlights: Node2D
@export var highlight_scene: PackedScene

@export var notices: VBoxContainer

@export var major_card_display: Control

@export var player1_info: Control
@export var player1_card_display : Control
@export var player2_info: Control
@export var player2_card_display : Control

var selected_piece: Piece2D

var game_active: bool = false

var cursor_over_game: bool = false
var disabled_selection: bool = false

var allow_quit: bool = true

var cur_selected_card: int = -1

var cur_display_cards_display: Node
var cur_display_card_tween: Tween
var cur_display_card: Node
var cur_display_card_exceptions: Array[Node]

var all_pieces: Array[Piece2D]

var rotating_board: bool = false
var rotate_start: Vector2

func _ready() -> void:
	GameManager.about_to_select_minor_card.connect(_on_about_to_select_minor_card)
	GameManager.clear_cards.connect(_on_clear_cards)
	GameManager.display_card.connect(card_selection.add_card)
	GameManager.show_cards.connect(_on_show_cards)
	GameManager.add_active_display_card.connect(_on_add_active_display_card)
	GameManager.card_score_changed.connect(_on_card_score_changed)
	
	GameManager.has_init.connect(_on_init)
	GameManager.turn_started.connect(_on_next_turn)
	GameManager.turn_ended.connect(_on_end_turn)
	
	GameManager.piece_added.connect(_on_piece_added)
	GameManager.piece_removed.connect(_on_piece_removed)
	
	GameManager.starting_actions.connect(_on_starting_actions)
	GameManager.taking_action.connect(_on_taking_action)
	GameManager.taking_actions_at.connect(_on_taking_actions_at)
	
	GameManager.action_processed.connect(_on_action_processed)
	GameManager.action_was_failed.connect(_on_action_failed)
	
	GameManager.players_in_check.connect(_on_players_in_check)
	GameManager.player_resigned.connect(_on_player_resigned)
	GameManager.player_has_won.connect(_on_player_won)
	GameManager.player_lost.connect(_on_player_lost)
	
	GameManager.game_stalemate.connect(_on_game_stalemate)
	
	GameManager.piece_taken.connect(_on_piece_taken)
	
	GameManager.grid_size_changed.connect(_on_grid_size_changed)
	
	GameManager.notice_received.connect(_on_notice_received)
	
	cursor.cell_selected.connect(select_cell)
	
	major_card_display.card_selected.connect(_on_display_card_selected.bind(major_card_display))
	player1_info.set_player(0)
	player1_info.max_progress = GameManager.CARD_SCORE_FOR_MINOR_CARD
	player1_card_display.card_selected.connect(_on_display_card_selected.bind(player1_card_display))
	player2_info.set_player(1)
	player2_info.max_progress = GameManager.CARD_SCORE_FOR_MINOR_CARD
	player2_card_display.card_selected.connect(_on_display_card_selected.bind(player2_card_display))
	
	var my_num = Lobby.get_first_player_num(multiplayer.get_unique_id())
	# If it's player 2, flip the board
	if my_num == 1:
		$BoardHolder.rotation = deg_to_rad(180)
	
	# If the game is not opened by a player, hide the quit button
	if not Lobby.is_player:
		$GameUI/BtnQuit.visible = false

func _on_init() -> void:
	cursor.board = GameManager.board
	
	if OS.is_debug_build():
		Debug.stats.add_property(GameManager.game_controller.CurrentGameState, "CurrentPlayerNum")

func _on_cursor_highlight_cell_updated(new_cell: Vector2i) -> void:
	_update_cursor_visibility()

func _update_cursor_visibility() -> void:
	# If cursor is outside range, then hide it
	cursor.visible = (GameManager.spaces_off_board(cursor.last_cell.x, cursor.last_cell.y) == 0) and cursor.active and cursor.visible_on_active and cursor_over_game and GameManager.in_game

func delete_selection() -> void:
	for child in action_highlights.get_children():
		child.queue_free()

func remove_selection() -> void:
	if selected_piece != null:
		selected_piece.actions_updated.disconnect(_on_actions_updated)
		selected_piece.set_selected(false)
	selected_piece = null
	delete_selection()

func update_selection() -> void:
	
	delete_selection()
	
	# If there is no selected piece, then ignore it
	if selected_piece == null:
		return
		
	var possible_actions: Array[Vector2i] = selected_piece.possible_actions
	# If the piece has no actions, then ignore it
	if possible_actions == null:
		return
	
	for action in possible_actions:
		var new_highlight: Node2D = highlight_scene.instantiate()
		
		new_highlight.board = board
		new_highlight.scale = Vector2(.85, .85)
		new_highlight.set_pos(action.x, action.y)
		action_highlights.add_child.call_deferred(new_highlight)

func set_selection(new_piece: Piece2D) -> void:
	if selected_piece == new_piece:
		update_selection()
		return
	remove_selection()
	selected_piece = new_piece
	selected_piece.set_selected(true)
	if selected_piece != null:
		selected_piece.actions_updated.connect(_on_actions_updated)
	update_selection()

func _on_actions_updated(piece: Piece2D) -> void:
	if piece != selected_piece:
		return
	update_selection()

func select_cell(cell_pos: Vector2i) -> void:
	# Ignore inputs from non-players
	if not Lobby.is_player:
		return
	if not GameManager.in_game:
		return
	# First check if the player is selecting an action
	var actions_to_take: Array = []
	if selected_piece != null and not disabled_selection: 
		# Check if there is an action being selected
		for action in selected_piece.possible_actions:
			if action == cell_pos:
				# If there is, request to act on that location
				var selected = selected_piece
				remove_selection()
				disabled_selection = true
				allow_quit = false
				GameManager.request_action.rpc(cell_pos, selected.id)
				return
	
	# Try to get the game mutex
	if not GameManager.game_mutex.try_lock():
		# If not, return, as it can't get the piece otherwise
		return
	
	# Get the first piece
	var piece: Piece = await GameManager.get_first_piece_at(cell_pos.x, cell_pos.y)
	# Unlock the mutex as it's not needed anymore
	GameManager.game_mutex.unlock()
	if piece == null:
		# If there's no piece, remove the selection
		remove_selection()
		return
	
	var item_node: Piece2D = GameManager.get_piece_2d(piece.Id)
	set_selection(item_node)


func show_cursor() -> void:
	cursor.visible_on_active = true
	_update_cursor_visibility()

func hide_cursor() -> void:
	cursor.visible_on_active = false


func update_arrow_visuals(player_num: int) -> void:
	player1_info.update_arrow_visual(player_num)
	player2_info.update_arrow_visual(player_num)

# Piece signals

func _on_piece_added(piece: Piece2D) -> void:
	all_pieces.append(piece)

func _on_piece_removed(piece: Piece2D) -> void:
	all_pieces.erase(piece)





# Game Signals

func _on_clear_cards() -> void:
	card_selection.visible = false
	card_selection.clear_cards()

func _on_show_cards() -> void:
	reset_display_card()
	card_selection.visible = true
	cur_selected_card = -1
	card_selection.show_cards()
	rotating_board = false

func _on_card_selected(card_num: int) -> void:
	# If the same, ignore
	if card_num == cur_selected_card:
		return
	# Tell the card selection to hold up the card, and put down the previous
	card_selection.put_card(card_num, true)
	card_selection.put_card(cur_selected_card, false)
	cur_selected_card = card_num



# Display card

func _on_about_to_select_minor_card(player_num: int) -> void:
	update_arrow_visuals(player_num)

func _on_display_card_selected(card: Node, display: Node) -> void:
	# If the selected card is the current one, reset
	if card == cur_display_card:
		reset_display_card()
		return
	set_display_card(card, display)

func reset_display_card() -> void:
	if cur_display_card == null or cur_display_cards_display == null:
		return
	
	cur_display_card_tween.kill()
	
	cur_display_card_exceptions.erase(cur_display_card)
	
	cur_display_cards_display.move_card(cur_display_card.card_id)
	cur_display_card.hover_enabled = true
	cur_display_card.enable_desc_scroll(false)
	
	cur_display_cards_display = null
	cur_display_card_tween = null
	cur_display_card = null

func set_display_card(card: Node, display: Node) -> void:
	reset_display_card()
	cur_display_card = card
	cur_display_cards_display = display
	
	cur_display_card.hover_enabled = false
	cur_display_card.enable_desc_scroll(true)
	
	cur_display_card_tween = create_tween()
	cur_display_card_tween.set_ease(Tween.EASE_OUT)
	cur_display_card_tween.set_trans(Tween.TRANS_CUBIC)
	cur_display_card_tween.tween_property(card, "global_position", Vector2(192, 320), 0.5)
	cur_display_card_tween.parallel().tween_property(card, "scale", Vector2(1, 1), 0.5)
	
	cur_display_card_exceptions.append(cur_display_card)
	

func _on_add_active_display_card(card: CardBase) -> void:
	var card_scene: PackedScene = preload("res://scenes/game/card/card.tscn")
	var new_card: Node = card_scene.instantiate()
	
	new_card.set_enabled(false)
	new_card.global_position = Vector2(-100,150)
	new_card.scale = Vector2(0.6, 0.6)
	
	# Add the information to the card
	new_card.set_card_name(GameManager.game_controller.GetCardName(card))
	new_card.set_card_description(GameManager.game_controller.GetCardDescription(card))
	new_card.enable_desc_scroll(false)
	new_card.set_card_image(GameManager.game_controller.GetCardImageLoc(card))
	
	add_child(new_card)
	
	var team_owner: int = card.TeamId
	
	# Tween the card so it appears, and moves onto the screen
	var tween: Tween = create_tween()
	
	# Move the card on to the screen
	tween.set_ease(Tween.EASE_OUT)
	tween.set_trans(Tween.TRANS_CUBIC)
	tween.tween_property(new_card, "global_position", Vector2(100, new_card.global_position.y), 0.5)
	tween.tween_interval(1)
	
	# If card is immediate use, hide the card off screen and delete
	if not card.DisplayCard:
		tween.tween_property(new_card, "global_position", Vector2(-100, new_card.global_position.y), 0.6)
		tween.tween_callback(new_card.queue_free)
		return
	var display: Node = major_card_display
	# Depending on team id, place differently on the board
	match team_owner:
		0:
			display = player1_card_display
		1:
	
			display = player2_card_display
	
	# After showing the card, add it to the display
	tween.tween_callback(display.add_card.bind(new_card))
	tween.tween_callback(display.move_cards.bind(cur_display_card_exceptions))

func _on_card_score_changed(player_num: int, new_score: int) -> void:
	if player_num == 0:
		player1_info.add_next_progress(new_score)
	else:
		player2_info.add_next_progress(new_score)


func _on_btn_use_pressed() -> void:
	# Only accept if all tweening is done
	if not card_selection.active_tweens.is_empty():
		return
	GameManager.select_card.rpc(cur_selected_card)



func _on_next_turn(new_player_num: int) -> void:
	# Re-enable the cursors visibility
	show_cursor()
	# Re-enable the selection and quit
	disabled_selection = false
	allow_quit = true
	# Update arrow visuals
	update_arrow_visuals(new_player_num)

func _on_end_turn() -> void:
	# Hide arrows between turns
	update_arrow_visuals(-1)

var just_started_actions: bool = false
func _on_starting_actions() -> void:
	# Hide the selection
	hide_cursor()
	# Remove selection first
	remove_selection()
	just_started_actions = true

func _reset_on_starting() -> void:
	# If just started actions, reset the current actions
	if just_started_actions:
		just_started_actions = false
		for cur_piece in all_pieces:
			cur_piece.reset_actions()

func _on_taking_action(action: ActionBase, piece: Piece) -> void:
	_reset_on_starting()
	allow_quit = false

func _on_taking_actions_at(action_location: Vector2i, piece: Piece) -> void:
	allow_quit = false

func _on_action_processed(action: ActionBase, piece: Piece) -> void:
	_reset_on_starting()

func _on_actions_processed(success: bool, action_location: Vector2i, piece: Piece) -> void:
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
	show_cursor()
	just_started_actions = false
	disabled_selection = false
	allow_quit = true

func _on_piece_taken(taken_piece: Piece, attacker: Piece) -> void:
	var remove_piece: Piece2D = GameManager.get_piece_2d(taken_piece.Id)
	if remove_piece == null:
		return
	GameManager.remove_piece_2d(remove_piece)
	remove_piece.queue_free()



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


func _on_players_in_check(players: Array) -> void:
	if players.has(GameManager.current_player_num):
		$CheckSound.play()


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



func _on_notice_received(text: String) -> void:
	notices.add_notice(text)



func to_lobby() -> void:
	GameManager.reset_game()
	if not multiplayer.has_multiplayer_peer() or multiplayer.multiplayer_peer is OfflineMultiplayerPeer:
		Lobby.remove_multiplayer_peer()
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


func _on_resign_btn_pressed() -> void:
	if not GameManager.in_game:
		to_lobby()
		return
	
	# TODO: Add a warning that the player has to accept.
	GameManager.resign_game.rpc()

func _on_request_draw_btn_pressed() -> void:
	if not GameManager.in_game:
		to_lobby()
		return

func _on_game_input_mouse_entered() -> void:
	cursor_over_game = true
	_update_cursor_visibility()


func _on_game_input_mouse_exited() -> void:
	cursor_over_game = false
	_update_cursor_visibility()

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
	cursor._process_input(event)
