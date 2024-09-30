extends Node

const MAJOR_VER: int = 0
const MINOR_VER: int = 4
const CHANGE_VER: int = 0

const MAJOR_VER_MIN: int = 0
const MINOR_VER_MIN: int = 4
const CHANGE_VER_MIN: int = 0

var DEFAULT_SERVER_IP: String = "127.0.0.1"
var DEFAULT_PORT: int = 9813
var MAX_CONNECTIONS: int = 5

var NAME_LENGTH_LIMIT: int = 13

var players: Dictionary = {}

var player_nums: Array[int] = [-1,-1]

var player_info: PlayerInfo = PlayerInfo.new(-1)

var doing_init: bool = false
var players_loaded: int = 0

var is_player: bool = false
var is_local: bool = false
var close_on_noone: bool = false



signal connection_successful()
signal connection_failed(reason: String)
signal connected_and_registered()
signal player_connected_server(peer_id: int) 			# Peer joins server
signal player_connected(player_info: Lobby.PlayerInfo)	# Peer registered
signal player_data_received(peer_id: int, player_info: Lobby.PlayerInfo)
signal player_num_updated(peer_id: int, player_num: int)
signal player_disconnected(peer_id: int)
signal server_disconnected()

## Channels
# 0 (Default) - Game
# 1 - Player data
# 2 - Messages

class PlayerInfo:
	
	var id: int = -1
	var name: String = ""
	var is_admin: bool = false
	
	var major_ver: int
	var minor_ver: int
	var change_ver: int
	
	func _init(id: int = -1, name: String = "Name", is_admin: bool = false):
		self.id = id
		self.name = name
		self.is_admin = is_admin
	
	func to_dictionary() -> Dictionary:
		return {
			"name": name,
			"is_admin": is_admin
		}
	
	static func _exists_in(dict: Dictionary, key: String, type: int) -> bool:
		if key not in dict:
			return false
		if not is_instance_of(dict[key], type):
			return false
		return true
	
	static func is_valid(dict: Dictionary) -> bool:
		if not _exists_in(dict, "name", TYPE_STRING):
			return false
		if not _exists_in(dict, "is_admin", TYPE_BOOL):
			return false
		return true
		
	static func from_dictionary(dict: Dictionary, id: int = -1) -> PlayerInfo:
		# Verify the data
		if not is_valid(dict):
			return null
		var new_info: PlayerInfo = PlayerInfo.new()
		new_info.id = id
		new_info.name = dict.name
		new_info.is_admin = dict.is_admin
		
		return new_info



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
	is_local = false
	doing_init = false
	players.clear()
	player_info.is_admin = false
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
		is_local = false
	else:
		peer = OfflineMultiplayerPeer.new()
		is_local = true
	# Initialise both player_nums to -1
	for i in range(player_nums.size()):
		player_nums[i] = -1
	multiplayer.multiplayer_peer = peer
	players.clear()
	player_info.id = multiplayer.get_unique_id()
	
	doing_init = false
	players_loaded = 0
	
	if is_player:
		# This player is admin
		player_info.is_admin = true
		_on_connected_ok()
	connection_successful.emit()

func leave_game():
	if multiplayer.multiplayer_peer:
		multiplayer.multiplayer_peer.close()
	players.clear()
	remove_multiplayer_peer()


func send_rpc(function: Callable) -> void:
	for key in players:
		function.rpc_id(key)

func get_valid_players() -> Array[PlayerInfo]:
	var valid_players: Array[PlayerInfo] = []
	for key in players:
		var player: PlayerInfo = players[key]
		valid_players.append(player)
	return valid_players

func remove_multiplayer_peer():
	for i in range(player_nums.size()):
		player_nums[i] = -1
	multiplayer.multiplayer_peer = null

func _on_player_connected(id: int):
	player_connected_server.emit(id)
	print("Player connected: " + str(id))

func _is_valid_version(major: int, minor: int, change: int) -> bool:
	if major < MAJOR_VER_MIN or major > MAJOR_VER:
		return false
	
	if minor < MINOR_VER_MIN or minor > MINOR_VER:
		return false
	
	if change < CHANGE_VER_MIN or change > CHANGE_VER:
		return false
	return true

func _on_connected_ok():
	# Tell the server I connected
	_register_player_server.rpc(player_info.to_dictionary(), MAJOR_VER, MINOR_VER, CHANGE_VER)
	connection_successful.emit()

@rpc("any_peer", "call_local", "reliable", 1)
func _register_player_server(new_player_info: Dictionary, major_ver: int, minor_ver: int, change_ver: int):
	# Only server runs this
	if not is_multiplayer_authority():
		return
	# Make sure data is valid
	var new_player_id: int = multiplayer.get_remote_sender_id()
	if not PlayerInfo.is_valid(new_player_info):
		# If into is not valid, just send out blank info
		new_player_info = PlayerInfo.new(new_player_id).to_dictionary()
	else:
		# Set the id, just to make sure
		new_player_info.id = new_player_id
	
	# Verify user's version.
	# Ignore server as it results in an error, though it should never fail
	if not _is_valid_version(major_ver, minor_ver, change_ver) and new_player_id != 1:
		print("Invalid game version: %s.%s.%s" % [major_ver, minor_ver, change_ver])
		# If invalid, disconnect the user
		multiplayer.multiplayer_peer.disconnect_peer(new_player_id)
		return
	
	# If it's the only player on a dedicated server, make them an admin
	if players.size() < 1 and not is_player:
		new_player_info.is_admin = true
	# Register the player for all other players, and register all
	# other players for this player
	for p_id in players:
		_register_player.rpc_id(new_player_id, p_id, players[p_id].to_dictionary())
	
	# As for the player themselves, just tell everyone
	_register_player.rpc(new_player_id, new_player_info)
	
	# Check the player nums. If there is a valid one,
	# then replace it
	var set_yet: bool = false
	for i in range(player_nums.size()):
		if not set_yet and player_nums[i] <= -1:
			set_player(new_player_id, i)
			set_yet = true
		else:
			_update_player_num.rpc_id(new_player_id, player_nums[i], i)

@rpc("authority", "call_local", "reliable", 1)
func _register_player(new_player_id: int, new_player_info: Dictionary):
	# Add to the dictionary
	players[new_player_id] = PlayerInfo.from_dictionary(new_player_info, new_player_id)
	
	# If this is this player's id, then we can run the function
	if new_player_id == multiplayer.get_unique_id():
		player_info = players[new_player_id]
		_registered()
	
	player_connected.emit(players[new_player_id])

@rpc("authority", "call_local", "reliable")
func _invalid_connection(reason: String) -> void:
	connection_failed.emit(reason)

func _registered():
	player_connected.emit(player_info)
	connected_and_registered.emit()

@rpc("authority", "call_local", "reliable", 1)
func _update_player(id: int, new_player_info: Dictionary):
	players[id] = PlayerInfo.from_dictionary(new_player_info, id)
	player_data_received.emit(players[id])

func get_player_info(id: int) -> Lobby.PlayerInfo:
	if players.has(id):
		return players[id]
	return null

func _on_player_disconnected(id):
	print("Player disconnected: %s" % [id])
	players.erase(id)
	player_disconnected.emit(id)
	if not is_multiplayer_authority():
		return
	
	for i in range(player_nums.size()):
		if id == player_nums[i]:
			set_player(-1, i)
			break
	
	# If not a player, close when there's 0 players
	if not is_player and close_on_noone:
		multiplayer.multiplayer_peer.close()

func _on_connected_fail():
	remove_multiplayer_peer()
	connection_failed.emit("Couldn't connect.")

func _on_server_disconnected():
	remove_multiplayer_peer()
	players.clear()
	server_disconnected.emit()

@rpc("any_peer", "call_local", "reliable", 1)
func request_data():
	if is_multiplayer_authority():
		# Loop through all the player data, and return it
		var other_id: int = multiplayer.get_remote_sender_id()
		for p_id in players:
			_update_player.rpc_id(other_id, players[p_id].to_dictionary())


### Game Lobby RPCs

func player_is_admin(player_id: int) -> bool:
	if player_id not in players:
		return false
	var player_data: PlayerInfo = players[player_id]
	return player_data.is_admin

func verify_name(name: String) -> String:
	if name.length() == 0:
		return ""
	if name.length() > NAME_LENGTH_LIMIT:
		name = name.substr(0,NAME_LENGTH_LIMIT)
	return name

func _update_name(id, name):
	players[id].name = name
	player_data_received.emit(players[id])


@rpc("authority", "call_remote", "reliable", 1)
func _change_name(id: int, name: String) -> String:
	var new_name: String = verify_name(name)
	if new_name.is_empty():
		return new_name
	_update_name(id, new_name)
	return new_name

@rpc("any_peer", "call_local", "reliable", 1)
func change_name(name: String):
	if not is_multiplayer_authority():
		return
	# Make sure name is valid
	var new_name: String = verify_name(name)
	if new_name.is_empty():
		return
	var other_id = multiplayer.get_remote_sender_id()
	_update_name(other_id, new_name)
	_change_name.rpc(other_id, new_name)

@rpc("authority", "call_local", "reliable")
func _update_player_num(id: int, player_num: int) -> void:
	player_nums[player_num] = id
	player_num_updated.emit(id, player_num)

func get_first_player_num(id: int) -> int:
	for i in range(player_nums.size()):
		if player_nums[i] == id:
			return i
	return -1

func get_player_nums(id: int) -> Array[int]:
	var return_vals: Array[int] = []
	for i in range(player_nums.size()):
		if player_nums[i] == id:
			return_vals.append(i)
	return return_vals

func get_player_id_from_num(num: int) -> int:
	if num < 0 or num >= player_nums.size():
		push_error("Tried to get id of player number %s, but valid player numbers are 0 - %s." % [num, player_nums.size()-1])
		return -1
	return player_nums[num]

@rpc("any_peer", "call_local", "reliable")
func request_set_player(id: int, player_num: int) -> void:
	# Multiplayer authority needs to check request
	if not is_multiplayer_authority():
		return
	
	# Only admin can set the players
	if not player_is_admin(multiplayer.get_remote_sender_id()):
		return
	
	set_player(id, player_num)

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

@rpc("any_peer", "call_local", "reliable")
func request_start_game():
	var player_id: int = multiplayer.get_remote_sender_id()
	# Multiplayer authority needs to check request
	if not is_multiplayer_authority():
		return
	
	# If player isn't an admin, ignore
	if not player_is_admin(player_id):
		send_error("You must be the lobby admin to start the game.", player_id)
		return
	
	# Make sure the player ids are valid
	for cur_id in player_nums:
		if not players.has(cur_id):
			send_error("Not all players have been set.", player_id)
			return
	
	# If player is an admin, start game
	setup_game()

func setup_game():
	# Only multiplayer authority can start
	if not is_multiplayer_authority():
		return
	
	# During setup, allow no connections
	multiplayer.multiplayer_peer.refuse_new_connections = true
	
	players_loaded = 0
	
	var valid_players = get_valid_players()
	
	print("Initialising game.")
	# Tell all clients to Init GameManager
	for player in valid_players:
		init_game.rpc_id(player.id)
	
	# Once game is init, init the board
	GameManager.init_board()
	
	if players.size() > players_loaded:
		await init_done
	print("Loading board.")
	# Once game init is done, send the board contents to the players
	players_loaded = 0
	done_init() # 1, as server is already loaded
	
	var board: Array = GameManager.board_to_array()
	# Send the array in groups of 5
	
	const JUMP_IND: int = 5
	var cur_ind: int = 0
	while cur_ind <= board.size():
		for player in valid_players:
			if player.id != 1:
				init_board.rpc_id(player.id, board.slice(cur_ind, cur_ind + JUMP_IND))
		cur_ind += JUMP_IND
	# Transmit an empty array, indicating the end of the board data
	for player in valid_players:
		if player.id != 1:
			init_board.rpc_id(player.id, [])
	
	if players.size() > players_loaded:
		await init_done
	
	print("Starting game.")
	# Once init is completely done, start the game
	var game_seed: int = GameManager.game_controller.GetGameSeed()
	for player in valid_players:
		start_game.rpc_id(player.id, await GameManager.game_controller.GetGameSeed())
	
	# Not that setup is completely done, allow new connections
	# TODO: Allow new connections to open the game while it's active from the lobby.
	multiplayer.multiplayer_peer.refuse_new_connections = false


@rpc("any_peer", "call_local", "reliable")
func done_init():
	if is_multiplayer_authority():
		players_loaded += 1
		print("%s / %s Players done." % [players_loaded, players.size()])
		if players_loaded >= players.size():
			init_done.emit()

@rpc("authority", "call_local", "reliable")
func init_game():
	doing_init = true
	# Unload current screen
	print(get_tree())
	
	# Then init game
	GameManager.init()
	# Done with init. Tell the server.
	done_init.rpc()

@rpc("authority", "call_remote", "reliable")
func init_board(board_content: Array):
	if not doing_init:
		return
	if board_content.is_empty():
		done_init.rpc()
		return
	GameManager.load_board(board_content)

@rpc("authority", "call_local", "reliable")
func start_game(game_seed: int):
	if not doing_init:
		return
	doing_init = false
	GameManager.start_game(game_seed)

### Game Initialisation RPCs

@rpc("any_peer", "call_local", "reliable")
func player_loaded():
	if multiplayer.is_server():
		players_loaded += 1
		if players_loaded == players.size():
			# Start game
			players_loaded = 0


### Other RPCs

const MESSAGE_LIMIT: int = 200

signal received_message(message: String)

@rpc("authority", "call_local", "reliable", 2)
func receive_message(message: String):
	received_message.emit(message)

func _send(message: String, id: int):
	if id <= -1:
		for player in get_valid_players():
			receive_message.rpc_id(player.id, message)
		return
	receive_message.rpc_id(id, message)

@rpc("any_peer", "call_local", "reliable", 2)
func send_message(message: String, id: int = -1):
	if not is_multiplayer_authority():
		return
	# Don't send an empty message
	if message.strip_edges().is_empty():
		return
	# Set character limit
	if message.length() > MESSAGE_LIMIT:
		message = message.substr(0, MESSAGE_LIMIT)
	var player_info: PlayerInfo = players[multiplayer.get_remote_sender_id()]
	var text: String = "<"
	if player_info.is_admin:
		text += "[color=salmon]"
	text += player_info.name
	if player_info.is_admin:
		text += "[/color]"
	# Finally, before adding the message, add a zero-width space after every "["
	# to disable user bbcode.
	message = message.replace("[", "[​")
	text += "> [color=lightgray]%s[/color]" % [message]
	_send(text, id)

func send_error(message: String, id: int = -1):
	if not is_multiplayer_authority():
		return
	
	var text: String = "[color=red]%s[/color]" % [message]
	_send(text, id)
