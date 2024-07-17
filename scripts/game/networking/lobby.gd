extends Node

const DEFAULT_SERVER_IP: String = "127.0.0.1"
const PORT: int = 9813
const MAX_CONNECTIONS: int = 5

const NAME_LENGTH_LIMIT: int = 13

var players: Dictionary = {}

var player_nums: Array[int] = [-1,-1]

var player_info: Dictionary = {"name":"Name"}

var players_loaded: int = 0

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


func join_game(address: String = ""):
	if address.is_empty():
		address = DEFAULT_SERVER_IP
	var peer = ENetMultiplayerPeer.new()
	var error = peer.create_client(address, PORT)
	if error:
		return error
	multiplayer.multiplayer_peer = peer

func create_game():
	var peer = ENetMultiplayerPeer.new()
	var error = peer.create_server(PORT, MAX_CONNECTIONS)
	if error:
		return error
	multiplayer.multiplayer_peer = peer
	
	players_loaded = 0
	
	players[multiplayer.get_unique_id()] = player_info
	set_player(multiplayer.get_unique_id(), 0)
	player_connected.emit(multiplayer.get_unique_id(), player_info)
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
	_register_player.rpc_id(id, player_info)
	
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

# Starting the game
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
func _update_player_num(id: int, player_num: int):
	player_nums[player_num] = id
	player_num_updated.emit(id, player_num)

func set_player(id: int, player_num: int):
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

func start_game():
	if not is_multiplayer_authority():
		return
	

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
