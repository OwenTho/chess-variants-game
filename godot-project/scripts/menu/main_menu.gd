extends Node

@export var ip_entry: LineEdit
@export var server_port_entry: LineEdit

@export var code_entry: LineEdit

var can_toggle_buttons: bool = true

func _ready():
	Lobby.connected_and_registered.connect(_on_connection_successful)
	ServerClient.PortReceived.connect(_on_port_received)
	ServerClient.Disconnected.connect(_on_server_disconnect)
	ServerClient.Error.connect(_on_server_error)

func check_entries() -> bool:
	if not ip_entry.text.is_empty() and not ip_entry.text.is_valid_ip_address():
		error_dialog("Invalid IP", "Please enter a valid IP.")
		return false
	
	var server_port: String = server_port_entry.text
	if not server_port.is_empty():
		if not server_port.is_valid_int():
			error_dialog("Invalid Port", "Please enter a valid Port (1024 - 49151, or it will default to %s)." % [Lobby.DEFAULT_PORT + 1])
			return false
	
		var server_port_int: int = int(server_port)
		if server_port_int < 1024 or server_port_int > 49151:
			error_dialog("Invalid Port", "Please enter a valid Port (1024 - 49151, or it will default to %s)." % [Lobby.DEFAULT_PORT + 1])
			return false
	return true

func _on_btn_play_pressed() -> void:
	# Disconnect signal to avoid loading lobby menu
	Lobby.connected_and_registered.disconnect(_on_connection_successful)
	# Start a local server
	# Port doesn't matter
	Lobby.create_game(false)
	# Set all players to the server
	for i in range(Lobby.player_nums.size()):
		Lobby.player_nums[i] = 1
	# Start the game
	Lobby.setup_game()

func disable_buttons() -> void:
	if not can_toggle_buttons:
		return
	for node in get_tree().get_nodes_in_group("btn_disable"):
		node.disabled = true

func enable_buttons() -> void:
	if not can_toggle_buttons:
		return
	for node in get_tree().get_nodes_in_group("btn_disable"):
		node.disabled = false

func _on_btn_quit_pressed() -> void:
	get_tree().quit()

var ip_joined: String

func _on_btn_server_pressed():
	if multiplayer.multiplayer_peer:
		multiplayer.multiplayer_peer.close()
	# Ensure IP and Port are valid
	if not check_entries():
		return
	ip_joined = ip_entry.text
	var server_port: int = int(server_port_entry.text)
	# If there are no values, then use the defaults
	if ip_entry.text.is_empty():
		ip_joined = ServerClient.DefaultIp
	if server_port_entry.text.is_empty():
		server_port = ServerClient.DefaultPort
	# Create a StreamPeerTcp and try to create a lobby
	ServerClient.Connected.connect(ServerClient.RequestCreate)
	ServerClient.ConnectToHost(ip_joined, server_port)
	disable_buttons()



func join_game(ip: String, port: int) -> void:
	print("Joining server %s:%s" % [ip_joined, port])
	var error = Lobby.join_game(ip_joined, port)
	if error != OK:
		print("Failed to join server: %s" % [error_string(error)])

func error_dialog(title: String, text: String) -> void:
	var new_dialog = AcceptDialog.new()
	new_dialog.title = title
	new_dialog.dialog_text = text
	new_dialog.close_requested.connect(new_dialog.queue_free)
	new_dialog.confirmed.connect(new_dialog.queue_free)
	add_child(new_dialog)
	new_dialog.popup_centered()

func _on_btn_join_pressed():
	if multiplayer.multiplayer_peer:
		multiplayer.multiplayer_peer.close()
	# Make sure inputs are valid
	if not check_entries():
		return
	# If Lobby code entry isn't 5 long, error
	if code_entry.text.length() != 5:
		error_dialog("Invalid Lobby Code.", "Lobby Code needs to be exactly 5 characters long.")
		return
	ip_joined = ip_entry.text
	var server_port: int = int(server_port_entry.text)
	# If there are no values, then use the defaults
	if ip_entry.text.is_empty():
		ip_joined = ServerClient.DefaultIp
	if server_port_entry.text.is_empty():
		server_port = ServerClient.DefaultPort
	disable_buttons()
	ServerClient.Connected.connect(ServerClient.RequestJoin.bind(code_entry.text))
	var error = ServerClient.ConnectToHost(ip_joined, server_port)
	if error:
		error_dialog("Invalid Connection", "Could not connect. Error: " + error_string(error))
		return

func _disconnect_signals() -> void:
	if ServerClient.Connected.is_connected(ServerClient.RequestCreate):
		ServerClient.Connected.disconnect(ServerClient.RequestCreate)
	if ServerClient.Connected.is_connected(ServerClient.RequestJoin):
		ServerClient.Connected.disconnect(ServerClient.RequestJoin)

func _on_server_disconnect() -> void:
	_disconnect_signals()
	enable_buttons()

func _on_server_error() -> void:
	_disconnect_signals()
	enable_buttons()

func _on_port_received(port: int) -> void:
	can_toggle_buttons = false
	join_game(ip_joined, port)

func _on_connection_successful():
	if get_tree():
		get_tree().change_scene_to_file("res://scenes/menu/lobby_menu.tscn")





func _on_advanced_label_meta_clicked(meta):
	$ServerControls/AdvancedSettings.visible = not $ServerControls/AdvancedSettings.visible
	# Clear the entries
	ip_entry.text = ""
	server_port_entry.text = ""

