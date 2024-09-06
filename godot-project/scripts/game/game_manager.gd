extends Node

const MAJOR_SELECT_COUNT: int = 3

var game_controller_script: CSharpScript = preload("res://scripts/game/GameController.cs")
var game_controller: Object

var game_scene: PackedScene = preload("res://scenes/game/game_screen.tscn")
var piece_scene: PackedScene = preload("res://scenes/game/piece/piece.tscn")

# Game
var in_game: bool
var game: Game
var piece_grid
var grid_upper_corner: Vector2i
var grid_lower_corner: Vector2i
var board: Board2D

# Mutex
var task_mutex: Mutex
var game_mutex: Mutex
var thread_mutex: Mutex

# Card Selection
var card_selector: CardSelector
var currently_selecting: int = -1

var current_given_cards: Array = []

var addons: Array[GameAddon]

signal has_init()

# Card signals
signal display_card(card_data: Dictionary)
signal clear_cards()
signal show_cards()
signal card_selected(card_id: int)

# Game signals
signal starting_next_turn(player_num: int)
signal next_turn(player_num: int)
signal end_turn()

signal taking_action(action: Node, piece: Node)
signal taking_actions_at(action_location: Vector2i, piece: Node)
signal action_processed(action: Node, piece: Node)
signal actions_processed_at(success: bool, action_location: Vector2i, piece: Node)
signal action_was_failed(reason: String)

signal player_has_won(player_num: int)
signal player_lost(player_num: int)

signal game_stalemate(stalemate_player: int)

signal piece_taken(taken_piece: Node, attacker: Node)

signal notice_received(message: String)

func _ready() -> void:
	Lobby.server_disconnected.connect(_on_server_disconnect)

func _on_server_disconnect() -> void:
	if game != null:
		game.to_menu()
	reset_game()
	#get_tree().change_scene_to_file("res://scenes/menu/main_menu.tscn")

func reset_game() -> void:
	# Before continuing, make sure none of the mutex are locked
	if game != null:
		game_controller.queue_free()
	if card_selector != null:
		card_selector.free()
	in_game = false
	board = null
	game = null
	piece_grid = null
	task_mutex = null
	game_mutex = null
	thread_mutex = null
	addons.clear()

func init() -> void:
	if game != null:
		game.queue_free()
	
	var cur_scene = get_tree().current_scene
	game = game_scene.instantiate()
	get_tree().root.add_child(game)
	get_tree().current_scene = game
	if cur_scene != null:
		cur_scene.queue_free()
	
	board = game.board
	
	# Initialise the game controller
	game_controller = game_controller_script.new()
	add_child(game_controller)
	
	# Initialise the card selector
	# Only the server has to initialise this
	if is_multiplayer_authority():
		card_selector = CardSelector.new()
		card_selector.game_controller = game_controller
		add_child(card_selector)
	
	
	# Initialise the game
	game_controller.FullInit(is_multiplayer_authority())
	piece_grid = game_controller.pieceGrid
	
	grid_upper_corner = game_controller.gridUpperCorner
	grid_lower_corner = game_controller.gridLowerCorner
	
	# Get the mutex
	task_mutex = game_controller.taskMutex
	game_mutex = game_controller.gameMutex
	thread_mutex = game_controller.threadMutex
	
	# Initialise the addons
	init_addons()
	
	setup_signals()
	
	has_init.emit()

func init_addons() -> void:
	# For simplicity, load them directly
	var promotion_scene = preload("res://scripts/game/addon/promotion/promotion.gd")
	add_addon("Promotion", promotion_scene.new())

func add_addon(name: String, addon: GameAddon) -> void:
	if addon in addons:
		push_error("Addon is already registered: %s" % [addon])
		return
	for other_addon in addons:
		if other_addon.name == name:
			push_error("Can't register new addon with name '%s' as it's already used by another addon.")
			return
	addons.append(addon)
	addon.name = name + "Addon"
	# Add as a child to the game scene
	if addon.get_parent() == null:
		game.add_child(addon)
	else:
		addon.reparent(game)
		push_warning("Addon already had a parent.")

func setup_signals() -> void:
	game_controller.NewTurn.connect(_on_next_turn)
	game_controller.ActionProcessed.connect(_on_action_processed)
	game_controller.ActionsProcessedAt.connect(_on_actions_processed_at)
	
	game_controller.CardNotice.connect(_on_card_notice)
	
	game_controller.PlayerLost.connect(_on_player_lost)
	game_controller.GameStalemate.connect(_on_game_stalemate)
	
	game_controller.PieceRemoved.connect(_on_piece_taken)
	game_controller.SendNotice.connect(send_notice)
	
	game_controller.UpperBoundChanged.connect(_on_upper_bound_changed)
	game_controller.LowerBoundChanged.connect(_on_lower_bound_changed)

func _on_upper_bound_changed(new_bound: Vector2i) -> void:
	grid_upper_corner = new_bound

func _on_lower_bound_changed(new_bound: Vector2i) -> void:
	grid_lower_corner = new_bound


func start_game(game_seed: int) -> void:
	in_game = true
	clear_cards.emit()
	# Ignore if not the server
	if not is_multiplayer_authority():
		return
	
	# Set the seed + state before starting
	set_seed.rpc(game_seed)
	
	# Connect signals to the card selector
	card_selector.before_new_selection.connect(_on_before_new_selection)
	card_selector.card_option_added.connect(_on_card_option_added)
	card_selector.selection_started.connect(_on_selection_started)
	card_selector.invalid_selection.connect(_on_invalid_selection)
	card_selector.card_selected.connect(_on_card_selected)
	card_selector.selection_done.connect(_on_selection_done)
	card_selector.all_selections_done.connect(_on_all_cards_selected.bind(game_seed))
	
	# Start the game by generating Major Cards that the players have to select
	for player_num in range(Lobby.player_nums.size()):
		card_selector.add_card_selection(player_num, game_controller.MajorCardDeck, MAJOR_SELECT_COUNT)
	card_selector.select()

func _on_before_new_selection() -> void:
	var send_id: int = Lobby.get_player_id_from_num(card_selector.currently_selecting)
	if send_id == -1:
		push_error("Can't send card data as no player is currently selecting a card.")
		return
	new_card_selection.rpc_id(send_id)

func _on_card_option_added(card_data) -> void:
	var send_id: int = Lobby.get_player_id_from_num(card_selector.currently_selecting)
	if send_id == -1:
		push_error("Can't send card data as no player is currently selecting a card.")
		return
	receive_card.rpc_id(send_id, card_data)

func _on_selection_started() -> void:
	var send_id: int = Lobby.get_player_id_from_num(card_selector.currently_selecting)
	if send_id == -1:
		push_error("Can't send card data as no player is currently selecting a card.")
		return
	display_cards.rpc_id(send_id)

func _on_selection_done() -> void:
	var send_id: int = Lobby.get_player_id_from_num(card_selector.currently_selecting)
	if send_id == -1:
		push_error("Can't send selection completion as no player is currently selecting a card.")
		return
	card_selection_done.rpc_id(send_id)

func _on_invalid_selection(card_num: int) -> void:
	var send_id: int = Lobby.get_player_id_from_num(card_selector.currently_selecting)
	if send_id == -1:
		push_error("Can't send invalid card rpc as no player is currently selecting a card.")
		return
	invalid_card.rpc_id(send_id, card_num)

func _on_card_selected(card) -> void:
	add_card(card)
	add_card_from_data.rpc(game_controller.ConvertCardToDict(card))

func _on_all_cards_selected(game_seed: int) -> void:
	# Disconnect the signals
	card_selector.before_new_selection.disconnect(_on_before_new_selection)
	card_selector.card_option_added.disconnect(_on_card_option_added)
	card_selector.selection_started.disconnect(_on_selection_started)
	card_selector.invalid_selection.disconnect(_on_invalid_selection)
	card_selector.card_selected.disconnect(_on_card_selected)
	card_selector.selection_done.disconnect(_on_selection_done)
	card_selector.all_selections_done.disconnect(_on_all_cards_selected)
	
	# RPC that game has started
	start_chess_game.rpc()


@rpc("authority", "call_local", "reliable")
func new_card_selection() -> void:
	clear_cards.emit()

@rpc("authority", "call_local", "reliable")
func receive_card(card_data: Dictionary) -> void:
	if not in_game:
		return
	if not "card_id" in card_data:
		push_error("Server sent card data without a card_id to display.")
		return
	if not "card_num" in card_data:
		push_error("Server sent card data without providing the id to return.")
		return
	var card = game_controller.MakeCardFromDict(card_data)
	# If it's null, ignore
	if card == null:
		push_error("Server sent card data with an unregistered card_id %s to display." % card_data.card_id)
		return
	
	# If it's not, get the data to display
	display_card.emit({
		"card_id": card_data.card_num,
		"name": card.GetCardName(),
		"image_loc": card.GetCardImageLoc(),
		"description": card.GetCardDescription()
	})
	# Free the card from memory now that it's used
	card.queue_free()

@rpc("authority", "call_local", "reliable")
func display_cards() -> void:
	if not in_game:
		return
	await get_tree().process_frame
	
	show_cards.emit()

@rpc("any_peer", "call_local", "reliable")
func select_card(card_number: int) -> void:
	if not is_multiplayer_authority():
		return
	if not in_game:
		return
	
	var player_id: int = multiplayer.get_remote_sender_id()
	var player_num: int = Lobby.get_player_num(player_id)
	
	# Ignore non-players
	if player_num == -1:
		return
	
	# Ignore if it's not the right player
	if Lobby.get_player_id_from_num(card_selector.currently_selecting) != player_id:
		return
	
	# Make sure seed state matches BEFORE card is added
	set_seed_state.rpc(game_controller.GetGameSeedState())
	# Tell card selector
	card_selector.select_card(card_number)


@rpc("authority", "call_remote", "reliable")
func add_card_from_data(card_data: Dictionary) -> void:
	if not in_game:
		return
	var card = game_controller.MakeCardFromDict(card_data)
	if card == null:
		push_error("Received an invalid card from the server.")
		return
	add_card(card)

func add_card(card) -> void:
	game_controller.AddCard(card)

@rpc("authority", "call_local", "reliable")
func card_selection_done() -> void:
	clear_cards.emit()

@rpc("authority", "call_local", "reliable")
func invalid_card(card_num: int) -> void:
	if not in_game:
		return







@rpc("authority", "call_local", "reliable")
func start_chess_game() -> void:
	if not in_game:
		return
	clear_cards.emit()
	game_controller.StartGame()
	game.game_active = true

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
	game_mutex.lock()
	for cell in piece_grid.cells:
		var cell_pos: Vector2i = cell.pos
		for item in cell.items:
			var this_item: Array = []
			# TODO: Update GridItem to have its own ID, which
			# id is saved to instead of pieceId for Pieces. This would be
			# for custom spaces.
			# This works for now, however, as there are only pieces.
			# Add the ID first
			if item.info != null:
				this_item.append(item.info.pieceId)
			else:
				this_item.append("invalid_id")
			# Then add the link ID
			this_item.append(item.linkId)
			# The team of the piece
			this_item.append(item.teamId)
			# The position of the item
			this_item.append(cell_pos)
			# The id of the item
			this_item.append(item.id)
			
			ret_array.append(this_item)
	game_mutex.unlock()
	return ret_array

func load_board(board_data: Array) -> void:
	for item in board_data:
		place_piece(item[0], item[1], item[2], item[3].x, item[3].y, item[4])

func place_piece(piece_id: String, link_id: int, team: int, x: int, y: int, id: int = -1) -> bool:
	# Request the game controller to make the piece
	var new_piece_data = await game_controller.PlacePiece(piece_id, link_id, team, x, y, id)
	
	# If it failed to place the piece, return false
	if new_piece_data == null:
		return false
	
	# Now that we have a new piece, initialise the Piece2D
	var new_piece: Piece2D = piece_scene.instantiate()
	
	# Add the piece data
	new_piece.piece_data = new_piece_data
	new_piece.board = board
	
	# Update the position and sprite
	new_piece.update_pos()
	new_piece.update_sprite()
	
	# Add node to tree
	get_tree().get_first_node_in_group("piece_holder").add_child(new_piece)
	
	return true

func place_matching(piece_id: String, link_id: int, x: int, y: int) -> void:
	place_piece(piece_id, link_id, 0, x, y)
	place_piece(piece_id, link_id, 1, x, grid_upper_corner.y - y)




func get_piece_id(id: int) -> Piece2D:
	for piece in get_tree().get_nodes_in_group("piece"):
		if piece.piece_data != null and piece.piece_data.id == id:
			return piece
	return null


func spaces_off_board(x: int, y: int) -> int:
	# If it's on the board, return 0
	var return_val: int = 0
	# Return the largest distance off the board
	if x < grid_lower_corner.x:
		return_val = max(return_val, abs(grid_lower_corner.x - x))
	elif x > grid_upper_corner.x:
		return_val = max(return_val, abs(grid_upper_corner.x - x))
	if y < grid_lower_corner.y:
		return_val = max(return_val, abs(grid_lower_corner.y - y))
	elif y > grid_upper_corner.y:
		return_val = max(return_val, abs(grid_upper_corner.y - y))
	return return_val

## Tasks

func game_controller_valid() -> bool:
	if game_controller == null || !is_instance_valid(game_controller):
		return false
	return true


func is_action_valid(action, piece) -> bool:
	if not game_controller_valid():
		return false
	return await game_controller.IsActionValid(action, piece)


func get_current_player() -> int:
	if not game_controller_valid():
		return -1
	return await game_controller.GetCurrentPlayer()


func get_piece(id: int) -> Object:
	if not game_controller_valid():
		return null
	return await game_controller.GetPiece(id)


func get_first_piece_at(x: int, y: int) -> Object:
	if not game_controller_valid():
		return null
	return await game_controller.GetFirstPieceAt(x,y)


func swap_piece_to(piece_id: int, id: String) -> void:
	game_controller.SwapPieceTo(piece_id, id)




### Events

func _on_next_turn(player_num: int) -> void:
	next_turn.emit(player_num)
	if is_multiplayer_authority():
		to_next_turn.rpc(player_num)
		_send_valid_actions()

func _send_valid_actions() -> void:
	if not in_game:
		return
	# Send all of the valid actions to the players
	game_mutex.lock()
	for piece in game_controller.GetAllPieces():
		_send_piece_actions(piece)
	game_mutex.unlock()

func _send_piece_actions(piece: Node) -> void:
	if piece == null:
		push_error("Tried to send actions of null piece.")
		return
	# Get all of the action locations
	var action_locations: Array[Vector2i] = []
	for action in piece.currentPossibleActions:
		# Only process the action if it's acting and valid
		if not action.valid or not action.acting:
			continue
		if action.actionLocation not in action_locations:
			action_locations.append(action.actionLocation)
	
	# Now send the action locations for the piece
	_receive_piece_actions.rpc(piece.id, action_locations)

@rpc("authority", "call_local", "reliable")
func _receive_piece_actions(piece_id: int, piece_actions: Array[Vector2i]) -> void:
	# Get the related Piece2D
	var piece: Piece2D = get_piece_id(piece_id)
	piece.set_actions(piece_actions)

func _on_action_processed(action: Node, piece) -> void:
	action_processed.emit(action, piece)
	if not is_multiplayer_authority():
		return
	take_action.rpc(game_controller.ActionToDict(action), piece.id)

func _on_actions_processed_at(success: bool, action_location: Vector2i, piece) -> void:
	actions_processed_at.emit(success, action_location, piece)
	if not is_multiplayer_authority():
		return
	if not success:
		var cur_player_id: int = Lobby.player_nums[await get_current_player()]
		failed_action.rpc_id(cur_player_id)
		return
	
	# Move to the next turn
	game_controller.NextTurn()


func _on_card_notice(card: Node, notice: String) -> void:
	print("%s has sent notice '%s'" % [card.GetCardName(), notice])
	for addon in addons:
		addon._handle_card_notice(card, notice)

func send_card_notice(card: Node, notice: String) -> void:
	game_controller.SendCardNotice(card, notice)



func _on_piece_taken(taken_piece, attacker) -> void:
	piece_taken.emit(taken_piece, attacker)


# TODO: Rather than calling player_won, instead remove the player from
# the game. When new turn is called, determine who wins at that point.
# This allows for draws (not the same as stalemates)
func _on_player_lost(player_num: int) -> void:
	if not is_multiplayer_authority():
		return
	
	player_won.rpc(2 - player_num)

func _on_game_stalemate(stalemate_player: int) -> void:
	if not is_multiplayer_authority():
		return
	
	game_statemate.rpc(stalemate_player)

func send_notice(player_target: int, text: String) -> void:
	if not is_multiplayer_authority():
		return
	# If it's -1, rpc to all
	if player_target == -1:
		receive_notice.rpc(text)
		return
	# Otherwise, get the id of the player
	var player_id: int = Lobby.player_nums[player_target]
	receive_notice.rpc_id(player_id, text)




### Chess Rpcs

@rpc("authority", "call_local", "reliable")
func set_seed(seed: int) -> void:
	game_controller.SetGameSeed(seed)

@rpc("authority", "call_local", "reliable")
func set_seed_state(state: int) -> void:
	game_controller.SetGameSeedState(state)

@rpc("any_peer", "call_local", "reliable")
func request_action(action_location: Vector2i, piece_id: int) -> void:
	# Only run if game is on
	if not in_game:
		return
	# Ignore if not authority
	if not is_multiplayer_authority():
		return
	
	var sender_id: int = multiplayer.get_remote_sender_id()
	
	# Get the player number of this player
	var player_num: int = Lobby.get_player_num(sender_id)
	# If it's -1, ignore
	if (player_num == -1):
		failed_action.rpc_id(sender_id)
		return
	
	# If it's the wrong player number, ignore
	var cur_player_num = await get_current_player()
	if Lobby.player_nums[cur_player_num] != sender_id:
		failed_action.rpc_id(sender_id, "It is not your turn.")
		return
	
	# Get piece
	var piece = await get_piece(piece_id)
	
	# If piece id is invalid, ignore
	if piece == null:
		failed_action.rpc_id(sender_id, "Piece not found.")
		return
	
	# Verify actions at that location
	var valid_actions: bool = true
	var possible_actions: Array = piece.currentPossibleActions
	
	# If there are no actions, ignore
	if possible_actions.size() == 0:
		failed_action.rpc_id(sender_id, "This piece has no actions to take.")
		return
	
	# Take the actions
	taking_actions_at.emit(action_location, piece)
	game_controller.TakeActionAt(action_location, piece)


@rpc("authority", "call_local", "reliable")
func failed_action(reason: String = "") -> void:
	# Only run if game is on
	if not in_game:
		return
	action_was_failed.emit(reason)

@rpc("authority", "call_remote", "reliable")
func take_action(action_dict: Dictionary, piece_id: int) -> void:
	# Only run if game is on
	if not in_game:
		return
	
	var action = game_controller.ActionFromDict(action_dict)
	var piece = await get_piece(piece_id)
	taking_action.emit(action, piece)
	game_controller.TakeAction(action, piece)

@rpc("authority", "call_remote", "reliable")
func take_action_at(action_location: Vector2i, piece_id: int) -> void:
	# Only run if game is on
	if not in_game:
		return
	var piece = await get_piece(piece_id)
	if piece != null:
		taking_actions_at.emit(action_location, piece)
		game_controller.TakeActionAt(action_location, piece)


@rpc("authority", "call_remote", "reliable")
func to_next_turn(new_player_num: int) -> void:
	# Only run if game is on
	if not in_game:
		return
	starting_next_turn.emit(new_player_num)
	game_controller.NextTurn(new_player_num)


@rpc("authority", "call_local", "reliable")
func player_won(winner: int) -> void:
	# Only run if game is on
	if not in_game:
		return
	# Reset game data, given that it's not needed anymore.
	reset_game()
	if not Lobby.is_player:
		# Dedicated servers unload the game scene
		get_tree().unload_current_scene()
		return
	player_has_won.emit(winner)


@rpc("authority", "call_local", "reliable")
func game_statemate(stalemate_player: int) -> void:
	if not in_game:
		return
	# Reset game data, given that it's not needed anymore.
	reset_game()
	if not Lobby.is_player:
		# Dedicated servers unload the game scene
		get_tree().unload_current_scene()
		return
	game_stalemate.emit(stalemate_player)


@rpc("authority", "call_local", "reliable", 2)
func receive_notice(text: String) -> void:
	# Only run if game is on
	if not in_game:
		return
	notice_received.emit(text)
