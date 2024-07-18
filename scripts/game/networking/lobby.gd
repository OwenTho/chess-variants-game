extends Node

const DEFAULT_SERVER_IP: String = "127.0.0.1"
const DEFAULT_PORT: int = 9813
const MAX_CONNECTIONS: int = 5

const NAME_LENGTH_LIMIT: int = 13

var players: Dictionary = {}

var player_nums: Array[int] = [-1,-1]

var player_info: Dictionary = {"name":"Name"}

var players_loaded: int = 0

var is_player: bool = false

signal connection_successful()
signal connection_failed()
signal player_connected(peer_id: int, player_info: Dictionary)
signal player_data_received(peer_id: int, player_info: Dictionary)
signal player_num_updated(peer_id: int, player_num: int)
signal player_disconnected(peer_id: int)
signal server_disconnected()

func _ready():
	multiplayer.peer_connected.connect(_on_player_connected)
	multiplayer.peer_disconnected.connect(_on_player_disconnected)
	multiplayer.connected_to_server.connect(_on_connected_ok)
	multiplayer.connection_failed.connect(_on_connected_fail)
	multiplayer.server_disconnected.connect(_on_server_disconnected)

func join_game(address: String = "", port: int = DEFAULT_PORT):
	# If server, ignore
	if OS.has_feature("dedicated_server"):
		return FAILED
	if address.is_empty():
		address = DEFAULT_SERVER_IP
	if port <= 1024 or port >= 49152:
		port = DEFAULT_PORT
	var peer = ENetMultiplayerPeer.new()
	var error = peer.create_client(address, port)
	if error:
		return error
	multiplayer.multiplayer_peer = peer
	

func create_game(online: bool = true, port: int = DEFAULT_PORT):
	if port <= 1024 or port >= 49152:
		port = DEFAULT_PORT
	var peer
	if online:
		peer = ENetMultiplayerPeer.new()
		var error = peer.create_server(port, MAX_CONNECTIONS)
		if error:
			return error
	else:
		peer = OfflineMultiplayerPeer.new()
	# Initialise both player_nums to -1
	for i in range(player_nums.size()):
		player_nums[i] = -1
	multiplayer.multiplayer_peer = peer
	
	players_loaded = 0
	
	if not OS.has_feature("dedicated_server"):
		is_player = true
		players[multiplayer.get_unique_id()] = player_info
		set_player(multiplayer.get_unique_id(), 0)
		player_connected.emit(multiplayer.get_unique_id(), player_info)
	else:
		is_player = false
	connection_successful.emit()

func leave_game():
	multiplayer.multiplayer_peer.close()
	players.clear()
	remove_multiplayer_peer()


func remove_multiplayer_peer():
	for i in range(player_nums.size()):
		player_nums[i] = -1
	multiplayer.multiplayer_peer = null

func _on_player_connected(id: int):
	if is_player:
		_register_player.rpc_id(id, player_info)
	
	print("PLAYER CONNECT: " + str(id))
	
	if is_multiplayer_authority():
		# Check the player nums. If there is a valid one,
		# then replace it
		var set_yet: bool = false
		for i in range(player_nums.size()):
			if not set_yet and player_nums[i] <= -1:
				set_player(id, i)
				set_yet = true
			else:
				_update_player_num.rpc_id(id, player_nums[i], i)

@rpc("any_peer", "call_local", "reliable")
func _register_player(new_player_info: Dictionary):
	var new_player_id = multiplayer.get_remote_sender_id()
	players[new_player_id] = new_player_info
	player_connected.emit(new_player_id, new_player_info)

@rpc("authority", "reliable")
func _update_player(id: int, new_player_info: Dictionary):
	players[id] = new_player_info
	player_data_received.emit(new_player_info)

func get_player_info(id: int):
	if players.has(id):
		return players[id]
	return null

func _on_player_disconnected(id):
	players.erase(id)
	player_disconnected.emit(id)
	if not is_multiplayer_authority():
		return
	
	for i in range(player_nums.size()):
		if id == player_nums[i]:
			set_player(-1, i)
			break

func _on_connected_ok():
	var peer_id = multiplayer.get_unique_id()
	players[peer_id] = player_info
	player_connected.emit(peer_id, player_info)
	connection_successful.emit()

func _on_connected_fail():
	remove_multiplayer_peer()
	connection_failed.emit()

func _on_server_disconnected():
	remove_multiplayer_peer()
	players.clear()
	server_disconnected.emit()

@rpc("any_peer", "call_local")
func request_data():
	if is_multiplayer_authority():
		# Loop through all the player data, and return it
		var other_id: int = multiplayer.get_remote_sender_id()
		for key in players:
			_update_player.rpc_id(other_id, key, players[key])


### Game Lobby RPCs

func verify_name(name) -> String:
	if name.length() < 0:
		name = "missing"
	if name.length() > NAME_LENGTH_LIMIT:
		name = name.substr(0,NAME_LENGTH_LIMIT)
	return name

func _update_name(id, name):
	players[id]["name"] = name
	player_data_received.emit(id, players[id])


@rpc("call_remote", "authority", "reliable", 1)
func _change_name(id, name):
	name = verify_name(name)
	_update_name(id, name)

@rpc("call_local", "any_peer", "reliable", 1)
func change_name(name):
	if not is_multiplayer_authority():
		return
	# Make sure name is valid
	name = verify_name(name)
	var other_id = multiplayer.get_remote_sender_id()
	_update_name(other_id, name)
	_change_name.rpc(other_id, name)

@rpc("authority", "call_local", "reliable")
func _update_player_num(id: int, player_num: int) -> void:
	player_nums[player_num] = id
	player_num_updated.emit(id, player_num)

func get_player_num(id: int) -> int:
	for i in range(player_nums.size()):
		if player_nums[i] == id:
			return i
	return -1

func set_player(id: int, player_num: int) -> void:
	if not is_multiplayer_authority():
		return
	
	
	# Try changing the player num
	var old_id: int = player_nums[player_num]
	# If it's the same id, just ignore
	if old_id == id:
		player_num_updated.emit(id, player_num)
		return
	
	# First check if the player is in any other slot
	if old_id >= 0:
		for i in range(player_nums.size()):
			if player_nums[i] == id:
				# If there is another slot, swap with the old id
				_update_player_num.rpc(-1, i)
				break
	
	_update_player_num.rpc(id, player_num)

signal init_done()

func start_game():
	if not is_multiplayer_authority():
		return
	
	players_loaded = 0
	
	# Tell all clients to Init GameManager
	init_game.rpc()
	
	# Once game is init, init the board
	GameManager.init_board()
	
	await init_done
	# Once game init is done, send the board contents to the players
	players_loaded = 1 # 1, as server is already loaded
	
	init_board.rpc(GameManager.board_to_array())
	
	await init_done
	
	# Once init is completely done, start the game

@rpc("any_peer", "call_local", "reliable")
func done_init():
	if is_multiplayer_authority():
		players_loaded += 1
		if players_loaded >= players.size():
			init_done.emit()

@rpc("authority", "call_local", "reliable")
func init_game():
	GameManager.init()
	# Done with init. Tell the server.
	done_init.rpc()

@rpc("authority", "call_remote", "reliable")
func init_board(board_content: Array):
	print(board_content)
	GameManager.load_board(board_content)
	done_init.rpc()

### Game Initialisation RPCs

@rpc("any_peer", "call_local", "reliable")
func player_loaded():
	if multiplayer.is_server():
		players_loaded += 1
		if players_loaded == players.size():
			# Start game
			players_loaded = 0






### Game RPCs

@rpc("call_local", "any_peer", "reliable")
func request_action(piece, action_pos: Vector2i) -> void:
	if not is_multiplayer_authority():
		return
	# Get the piece
	
	# Calculate the piece's actions
	
	# Verify the action
	
	# If valid, act
	receive_action(piece, action_pos)
	

func send_action(piece, action_pos: Vector2i) -> void:
	if not is_multiplayer_authority():
		return
	
	# Send to all peers
	receive_action.rpc(piece, action_pos)

@rpc("call_local", "authority", "reliable")
func receive_action(piece, action_pos: Vector2i) -> void:
	pass
	# Get the piece
	
	# Calculate the piece's actions
	
	# Get the actions at that location, and act on the piece.
	





### Other RPCs
