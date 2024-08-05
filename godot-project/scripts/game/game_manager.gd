extends Node

const MAJOR_SELECT_COUNT: int = 3

var game_controller_script: CSharpScript = preload("res://scripts/game/GameController.cs")
var game_controller: Object

var game_scene: PackedScene = preload("res://scenes/game/game_screen.tscn")
var piece_scene: PackedScene = preload("res://scenes/game/piece/piece.tscn")

var in_game: bool
var game: Game
var grid
var grid_size: Vector2i
var board: Board2D

var task_mutex: Mutex
var game_mutex: Mutex
var thread_mutex: Mutex

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

signal taking_action_at(action_location: Vector2i, piece)
signal action_processed(success: bool, action_location: Vector2i, piece)
signal action_was_failed(reason: String)

signal player_has_won(player_num: int)
signal player_lost(player_num: int)
signal piece_taken(taken_piece, attacker)

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
	in_game = false
	board = null
	game = null
	grid = null
	task_mutex = null
	game_mutex = null
	thread_mutex = null

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
	
	
	# Initialise the game
	game_controller.FullInit(is_multiplayer_authority())
	grid = game_controller.grid
	
	grid_size = game_controller.gridSize
	
	# Get the mutex
	task_mutex = game_controller.taskMutex
	game_mutex = game_controller.gameMutex
	thread_mutex = game_controller.threadMutex
	
	setup_signals()
	
	has_init.emit()

func setup_signals() -> void:
	game_controller.NewTurn.connect(_on_next_turn)
	game_controller.ActionProcessed.connect(_on_action_processed)
	
	game_controller.PlayerLost.connect(_on_player_lost)
	
	game_controller.PieceRemoved.connect(_on_piece_taken)
	game_controller.SendNotice.connect(send_notice)

var currently_selecting: int = -1

func start_game(game_seed: int) -> void:
	in_game = true
	clear_cards.emit()
	# Ignore if not the server
	if not is_multiplayer_authority():
		return
	
	# Start the game by generating 3 Major Cards that the players have to select
	var selected_cards: Array = []
	for i in range(Lobby.player_nums.size()):
		var player_id: int = Lobby.get_player_id_from_num(i)
		if player_id == -1:
			continue
		var player_cards: Array[Node] = []
		for j in range(MAJOR_SELECT_COUNT):
			# Pull a new card
			var new_card = game_controller.PullMajorCard()
			# If new card is null, break as there is no more cards available
			if new_card == null:
				break
			# Temporarily add as a child to avoid possible memory leak
			game.add_child(new_card)
			player_cards.append(new_card)
			# Send the card to the player
			var card_data: Dictionary = game_controller.ConvertCardToDict(new_card)
			# Add the card number
			card_data.card_num = j
			receive_card.rpc_id(player_id, card_data)
		
		# If there are no cards, break out of the loop
		if player_cards.size() == 0:
			break
		
		currently_selecting = i
		# Tell the player to display the cards
		display_cards.rpc_id(player_id)
		# Wait for the player to select the card
		var selected_card: int = -1
		while selected_card < 0 or selected_card >= player_cards.size():
			selected_card = await card_selected
			if selected_card < 0 or selected_card >= player_cards.size():
				invalid_card.rpc_id(player_id)
		
		# Free the unused cards
		for j in range(player_cards.size()):
			if j != selected_card:
				game_controller.ReturnMajorCard(player_cards[j])
				player_cards[j].queue_free()
		# Remove the child now that we can add it to the game
		game.remove_child(player_cards[selected_card])
		var card_data = game_controller.ConvertCardToDict(player_cards[selected_card])
		if card_data == null:
			continue
		# Add the card to the server
		add_card(player_cards[selected_card])
		# Tell all players to add the card, if it's a valid selection
		add_card_from_data.rpc(card_data)
	currently_selecting = -1
	# Now that the cards are initialised, finally start the chess game
	start_chess_game.rpc(game_seed)

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
		"name": card.GetName(),
		"image_loc": card.GetImageLoc(),
		"description": card.GetDescription()
	})
	# Free the card from memory now that it's used
	card.queue_free()

@rpc("authority", "call_local", "reliable")
func display_cards() -> void:
	if not in_game:
		return
	await get_tree().process_frame
	
	game.card_selection.show_cards()
	
	# TEMP: Wait 5 seconds and then select first card
	await get_tree().create_timer(3.0).timeout
	
	clear_cards.emit()
	await get_tree().process_frame
	select_card.rpc(0)

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
	if Lobby.player_nums[currently_selecting] != player_id:
		return
	# Signal that card was selected
	card_selected.emit(card_number)


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
func invalid_card() -> void:
	if not in_game:
		return







@rpc("authority", "call_local", "reliable")
func start_chess_game(game_seed: int) -> void:
	print("Start game")
	if not in_game:
		return
	clear_cards.emit()
	game_controller.StartGame(game_seed)
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
	
	# new_piece.add_child(new_piece_data)
	
	# Update the position and sprite
	new_piece.update_pos()
	
	# Add node to tree
	get_tree().get_first_node_in_group("piece_holder").add_child(new_piece)
	
	print("Placed %s (id: %s) at %s,%s" % [piece_id, id, x, y])
	return true

func place_matching(piece_id: String, id: int, x: int, y: int) -> void:
	place_piece(piece_id, id, 0, x, y)
	place_piece(piece_id, id, 1, x, game_controller.gridSize.y - y - 1)




func get_piece_id(id: int) -> Piece2D:
	for piece in get_tree().get_nodes_in_group("piece"):
		if piece.piece_data != null and piece.piece_data.id == id:
			return piece
	return null


func spaces_off_board(x: int, y: int) -> int:
	# If it's on the board, return 0
	var return_val: int = 0
	# Return the largest distance off the board
	if x < 0:
		return_val = max(return_val, abs(x))
	elif x >= grid_size.x:
		return_val = max(return_val, abs(grid_size.x - 1 - x))
	if y < 0:
		return_val = max(return_val, abs(y))
	elif y >= grid_size.y:
		return_val = max(return_val, abs(grid_size.y - 1 - y))
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



func _on_action_processed(success: bool, action_location: Vector2i, piece) -> void:
	action_processed.emit()
	if not is_multiplayer_authority():
		return
	if not success:
		var cur_player_id: int = Lobby.player_nums[await get_current_player()]
		failed_action.rpc_id(cur_player_id)
		return
	# Tell everyone to take the action
	take_action_at.rpc(action_location, piece.id)
	
	game_controller.NextTurn()


func _on_piece_taken(taken_piece, attacker) -> void:
	piece_taken.emit(taken_piece, attacker)


# TODO: Rather than calling player_won, instead remove the player from
# the game. When new turn is called, determine who wins at that point.
# This allows for draws (not the same as stalemates)
func _on_player_lost(player_num: int) -> void:
	if not is_multiplayer_authority():
		return
	
	player_won.rpc(2 - player_num)


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
	taking_action_at.emit(action_location, piece)
	game_controller.TakeActionAt(action_location, piece)


@rpc("authority", "call_local", "reliable")
func failed_action(reason: String = "") -> void:
	# Only run if game is on
	if not in_game:
		return
	action_was_failed.emit(reason)


@rpc("authority", "call_remote", "reliable")
func take_action_at(action_location: Vector2i, piece_id: int) -> void:
	# Only run if game is on
	if not in_game:
		return
	var piece = await get_piece(piece_id)
	if piece != null:
		taking_action_at.emit(action_location, piece)
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
		# Leave the game scene
		get_tree().unload_current_scene()
		return
	player_has_won.emit(winner)


@rpc("authority", "call_local", "reliable", 2)
func receive_notice(text: String) -> void:
	# Only run if game is on
	if not in_game:
		return
	notice_received.emit(text)
