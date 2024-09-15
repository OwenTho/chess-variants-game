extends Node

const PLAYER_COUNT: int = 2

const MAJOR_SELECT_COUNT: int = 3

const CARD_SCORE_PER_TURN: int = 1
const CARD_SCORE_PER_PIECE_TAKEN: int = 2
const CARD_SCORE_FOR_MINOR_CARD: int = 8

signal has_init()

# Card signals
signal about_to_select_minor_card(player_num: int)
signal display_card(card_data: Dictionary)
signal clear_cards()
signal show_cards()
signal card_selected(card_id: int)
signal add_active_display_card(card: CardBase)
signal card_score_changed(player_num: int, new_score: int)

# Game signals
signal starting_next_turn(player_num: int)
signal turn_started(player_num: int)
signal turn_ended()

signal piece_added(piece: Piece2D)
signal piece_removed(piece: Piece2D)

signal starting_actions()
signal taking_action(action: ActionBase, piece: Piece)
signal taking_actions_at(action_location: Vector2i, piece: Piece)
signal action_processed(action: ActionBase, piece: Piece)
signal actions_processed_at(success: bool, action_location: Vector2i, piece: Piece)
signal action_was_failed(reason: String)

signal player_resigned(player_num: int)
signal player_has_won(player_num: int)
signal player_lost(player_num: int)

signal game_stalemate(stalemate_player: int)

signal piece_taken(taken_piece: Piece, attacker: Piece)

signal notice_received(message: String)


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
var current_player_num: int = -1

# Mutex
var task_mutex: Mutex
var game_mutex: Mutex
var thread_mutex: Mutex

# Card Selection
var card_selector: CardSelector
var currently_selecting: int = -1

var card_score: Array[int] = []

var current_given_cards: Array[CardBase] = []

# Addons
var addons: Array[GameAddon]


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
	current_player_num = -1
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
	game_controller.FullInit(is_multiplayer_authority(), PLAYER_COUNT)
	current_player_num = 0
	piece_grid = game_controller.pieceGrid
	
	grid_upper_corner = game_controller.gridUpperCorner
	grid_lower_corner = game_controller.gridLowerCorner
	
	# Get the mutex
	task_mutex = game_controller.taskMutex
	game_mutex = game_controller.gameMutex
	thread_mutex = game_controller.threadMutex
	
	card_score.clear()
	for i in range(PLAYER_COUNT):
		card_score.append(0)
	
	# Initialise the addons
	init_addons()
	
	setup_signals()
	
	has_init.emit()

func init_addons() -> void:
	# For simplicity, load them directly
	var promotion_script = preload("res://scripts/game/addon/promotion/promotion.gd")
	add_addon("Promotion", promotion_script.new())
	var change_piece_script = preload("res://scripts/game/addon/change_piece/change_piece.gd")
	add_addon("Change Piece", change_piece_script.new())

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
	game_controller.TurnStarted.connect(_on_turn_started)
	game_controller.TurnEnded.connect(_on_turn_ended)
	
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
	
	# If no major cards will be added, wait for data to prepare for C#.
	# Normally it would have time as the players have to select the cards.
	if MAJOR_SELECT_COUNT == 0:
		await get_tree().create_timer(0.2).timeout
	
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
		card_selector.add_card_selection(player_num, game_controller.MajorCardDeck, MAJOR_SELECT_COUNT, -1)
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

func _on_card_selected(card: CardBase) -> void:
	if card == null:
		return
	add_card(card)
	add_card_from_data.rpc(game_controller.ConvertCardToDict(card))
	await game_controller.CardAdded

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
	var card: CardBase = game_controller.MakeCardFromDict(card_data)
	if card == null:
		push_error("Received an invalid card from the server.")
		return
	add_card(card)
	await game_controller.CardAdded

func add_card(card: CardBase) -> void:
	# Add visually to the game
	add_active_display_card.emit(card)
	
	# Add to the game
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
	var new_piece_data: Piece = await game_controller.PlacePiece(piece_id, link_id, team, x, y, id)
	
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
	
	piece_added.emit(new_piece)
	
	return true

func remove_piece_2d(piece: Piece2D) -> void:
	if piece == null:
		return
	piece.queue_free()
	piece_removed.emit(piece)

func remove_piece_2d_by_id(id: int) -> void:
	remove_piece_2d(get_piece_2d(id))

func place_matching(piece_id: String, link_id: int, x: int, y: int) -> void:
	place_piece(piece_id, link_id, 0, x, y)
	place_piece(piece_id, link_id, 1, x, grid_upper_corner.y - y)




func get_piece_2d(id: int) -> Piece2D:
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

func unsafe_get_current_player() -> int:
	if not game_controller_valid():
		return -1
	return game_controller.UnsafeGetCurrentPlayer()


func get_piece_info(info_id: String) -> PieceInfo:
	if not game_controller_valid():
		return null
	return game_controller.GetPieceInfo(info_id)


func get_piece(id: int) -> Piece:
	if not game_controller_valid():
		return null
	return await game_controller.GetPiece(id)

func unsafe_get_piece(id: int) -> Piece:
	if not game_controller_valid():
		return null
	return game_controller.UnsafeGetPiece(id)


func get_king_pieces() -> Array:
	if not game_controller_valid():
		return []
	return await game_controller.GetKingPieces()

func unsafe_get_king_pieces() -> Array:
	if not game_controller_valid():
		return []
	return game_controller.UnsafeGetKingPieces()

func get_first_piece_at(x: int, y: int) -> Piece:
	if not game_controller_valid():
		return null
	return await game_controller.GetFirstPieceAt(x,y)

func unsafe_get_first_piece_at(x: int, y: int) -> Piece:
	if not game_controller_valid():
		return null
	return game_controller.UnsafeGetFirstPieceAt(x,y)


func get_pieces_by_link_id(link_id: int) -> Array:
	if not game_controller_valid():
		return []
	return await game_controller.GetPiecesByLinkId(link_id)

func unsafe_get_pieces_by_link_id(link_id: int) -> Array:
	if not game_controller_valid():
		return []
	return game_controller.UnsafeGetPiecesByLinkId(link_id)


func swap_piece_to(piece_id: int, id: String) -> void:
	game_controller.SwapPieceTo(piece_id, id)



### Card Score

func add_card_score(player_num: int, score: int) -> void:
	# Ignore if not in game
	if not in_game:
		return
	# Ignore if not server
	if not is_multiplayer_authority():
		return
	# Make sure player_num is valid
	if player_num < 0 or player_num >= PLAYER_COUNT:
		push_error("Failed to add score to player num %s, as there is only %s players. (0-%s)" % [player_num, PLAYER_COUNT, PLAYER_COUNT-1])
		return
	_update_card_score_server(player_num, card_score[player_num] + score)

func _update_card_score_server(player_num: int, score: int) -> void:
	card_score[player_num] = score
	_update_card_score.rpc(player_num, card_score[player_num])
	card_score_changed.emit(player_num, card_score[player_num])

@rpc("authority", "call_remote", "reliable", 1)
func _update_card_score(player_num: int, score: int) -> void:
	if not in_game:
		return
	if player_num < 0 or player_num >= PLAYER_COUNT:
		push_error("Server tried to set card score of player %s, when there is no player with that number." % [player_num])
		return
	card_score[player_num] = score
	card_score_changed.emit(player_num, score)




### Events

func _on_turn_started(player_num: int) -> void:
	current_player_num = player_num
	turn_started.emit(player_num)
	
	# Then send actions
	if is_multiplayer_authority():
		to_next_turn.rpc(player_num)
		_send_valid_actions()




func _on_turn_ended(old_player_num: int, new_player_num: int) -> void:
	# Only continue if game is still active
	if not in_game:
		return
	# Add score to the player
	add_card_score(old_player_num, CARD_SCORE_PER_TURN)
	turn_ended.emit()
	
	current_player_num = new_player_num
	send_notice(-1, "Player %s's turn." % [new_player_num + 1])
	
	# Before moving to next turn, if the NEW player has
	# enough card score, give them a choice of card.
	# This only happens once per turn (so if score card is high enough
	# for two, it is given next turn)
	await send_minor_card_options(new_player_num)
	
	# Move to the next turn
	game_controller.NextTurn()

# Signal for indicating that all minor card selection is finished
# This includes waiting for the cards to be added, and whatever Waits the
# cards themselves do.
signal _minor_card_selection_is_finished()

func send_minor_card_options(player_num: int) -> void:
	# Only if score is high enough
	if card_score[player_num] < CARD_SCORE_FOR_MINOR_CARD:
		return
	# And no one is selecting
	if currently_selecting != -1:
		return
	
	card_score[player_num] -= CARD_SCORE_FOR_MINOR_CARD
	_update_card_score_server(player_num, card_score[player_num])
	# Tell clients to add the arrow visual
	# (so that it's more obvious who's selecting the card)
	_about_to_select_minor_card.rpc(player_num)
	
	# Connect signals to the card selector
	card_selector.before_new_selection.connect(_on_before_new_selection)
	card_selector.card_option_added.connect(_on_card_option_added)
	card_selector.selection_started.connect(_on_selection_started)
	card_selector.invalid_selection.connect(_on_invalid_selection)
	card_selector.card_selected.connect(_on_card_selected)
	card_selector.selection_done.connect(_on_selection_done)
	card_selector.all_selections_done.connect(_on_minor_card_selection_done)
	
	# Start the game by generating Major Cards that the players have to select
	var selection_info = card_selector.create_custom_selection(player_num, player_num)
	# Rule Deck
	selection_info.add_card_getter(CardSelector.CustomSelectionDeck.new(game_controller.MinorCardDeck, false))
	# Piece Card Factory
	selection_info.add_card_getter(CardSelector.CustomSelectionFactory.new(game_controller.ChangePieceFactory))
	# Space Card Factory
	# selection_info.add_card_getter(CardSelector.CustomSelectionFactory.new(game_controller.SpaceFactory))
	
	card_selector.add_custom_selection(selection_info)
	
	if await card_selector.select():
		await _minor_card_selection_is_finished

@rpc("authority", "call_local", "reliable")
func _about_to_select_minor_card(player_num: int) -> void:
	about_to_select_minor_card.emit(player_num)

func _on_minor_card_selection_done() -> void:
	card_selector.before_new_selection.disconnect(_on_before_new_selection)
	card_selector.card_option_added.disconnect(_on_card_option_added)
	card_selector.selection_started.disconnect(_on_selection_started)
	card_selector.invalid_selection.disconnect(_on_invalid_selection)
	card_selector.card_selected.disconnect(_on_card_selected)
	card_selector.selection_done.disconnect(_on_selection_done)
	card_selector.all_selections_done.disconnect(_on_minor_card_selection_done)
	
	currently_selecting = -1
	
	_minor_card_selection_is_finished.emit()





func _send_valid_actions() -> void:
	if not in_game:
		return
	# Send all of the valid actions to the players
	game_mutex.lock()
	for piece in game_controller.GetAllPieces():
		_send_piece_actions(piece)
	game_mutex.unlock()

func _send_piece_actions(piece: Piece) -> void:
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
	var piece: Piece2D = get_piece_2d(piece_id)
	piece.set_actions(piece_actions)

func _on_action_processed(action: ActionBase, piece: Piece) -> void:
	action_processed.emit(action, piece)
	if not is_multiplayer_authority():
		return
	take_action.rpc(game_controller.ActionToDict(action), piece.id)

func _on_actions_processed_at(success: bool, action_location: Vector2i, piece: Piece) -> void:
	actions_processed_at.emit(success, action_location, piece)
	if not is_multiplayer_authority():
		return
	if not success:
		var cur_player_id: int = Lobby.player_nums[await get_current_player()]
		failed_action.rpc_id(cur_player_id)
		return
	
	# End the current turn
	game_controller.EndTurn()

func _on_card_notice(card: CardBase, notice: String) -> void:
	print("%s Card has sent notice '%s'" % [card.GetCardName(), notice])
	for addon in addons:
		addon._handle_card_notice(card, notice)

func send_card_notice(card: CardBase, notice: String) -> void:
	game_controller.SendCardNotice(card, notice)



func _on_piece_taken(taken_piece: Piece, attacker: Piece) -> void:
	piece_taken.emit(taken_piece, attacker)
	# Add card score to the player that lost the piece
	# It is this way, as they lose a piece but can get a card sooner.
	# It is this way, rather than the person taking the piece, as otherwise
	# people can aim to take pieces for their benefit, whereas this forces
	# players to be more cautious about taking pieces.
	add_card_score(taken_piece.teamId, CARD_SCORE_PER_PIECE_TAKEN)



# TODO: Rather than calling player_won, instead remove the player from
# the game. When new turn is called, determine who wins at that point.
# This allows for draws (not the same as stalemates)
func _on_player_lost(player_num: int) -> void:
	if not is_multiplayer_authority():
		return
	
	player_lost.emit(player_num)
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
	# If currently selecting a card, disallow action
	if currently_selecting != -1:
		failed_action.rpc_id(sender_id)
		return
	
	# Get the player number of this player
	var player_num: int = Lobby.get_first_player_num(sender_id)
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
	start_actions.rpc()
	game_controller.TakeActionAt(action_location, piece)


@rpc("authority", "call_local", "reliable")
func failed_action(reason: String = "") -> void:
	# Only run if game is on
	if not in_game:
		return
	action_was_failed.emit(reason)

var cur_actions: Array = []

func _free_actions() -> void:
	for action in cur_actions:
		action.queue_free()
	cur_actions.clear()

@rpc("authority", "call_local", "reliable")
func start_actions() -> void:
	# Only run if game is on
	if not in_game:
		return
	starting_actions.emit()
	_free_actions()

@rpc("authority", "call_remote", "reliable")
func take_action(action_dict: Dictionary, piece_id: int) -> void:
	# Only run if game is on
	if not in_game:
		return
	
	var action = game_controller.ActionFromDict(action_dict)
	add_child(action)
	cur_actions.append(action)
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
	_free_actions()
	game_controller.NextTurn(new_player_num)


@rpc("any_peer", "call_local", "reliable")
func resign_game() -> void:
	# Only run if game is on
	if not in_game:
		return
	# Only run if the server
	if not is_multiplayer_authority():
		return
	
	var requester_id: int = multiplayer.get_remote_sender_id()
	var nums = Lobby.get_player_nums(requester_id)
	
	# If the id doesn't match any player nums, ignore
	if nums.size() == 0:
		return
	
	# If current player num is -1, the player can't resign yet
	if current_player_num == -1:
		receive_notice.rpc_id(requester_id, "Can't resign yet.")
		return
	
	# By default, use the first number
	var lose_player_id: int = nums[0]
	
	# If there is more than one player num, check if one is the current player.
	if nums.size() > 1:
		if not nums.has(current_player_num):
			# If it's not the current player, tell them to resign on their turn
			receive_notice.rpc_id(requester_id, "Can only resign on one of your turns.")
			return
		lose_player_id = current_player_num
	
	if lose_player_id <= -1:
		receive_notice.rpc_id(requester_id, "Could not get a valid player number to resign. (This should not happen)")
		return
	
	resign_player.rpc(lose_player_id)

func _game_end() -> bool:
	# Reset game data, given that it's not needed anymore.
	reset_game()
	if not Lobby.is_player:
		# Dedicated servers unload the game scene
		get_tree().unload_current_scene()
		return false
	return true

@rpc("authority", "call_local", "reliable")
func resign_player(player_num: int) -> void:
	# Only run if game is on
	if not in_game:
		return
	receive_notice("Player %s has resigned." % [player_num+1])
	if _game_end():
		player_resigned.emit(player_num)

@rpc("authority", "call_local", "reliable")
func player_won(winner: int) -> void:
	# Only run if game is on
	if not in_game:
		return
	if _game_end():
		player_has_won.emit(winner)


@rpc("authority", "call_local", "reliable")
func game_statemate(stalemate_player: int) -> void:
	if not in_game:
		return
	if _game_end():
		game_stalemate.emit(stalemate_player)


@rpc("authority", "call_local", "reliable", 2)
func receive_notice(text: String) -> void:
	# Only run if game is on
	if not in_game:
		return
	notice_received.emit(text)
