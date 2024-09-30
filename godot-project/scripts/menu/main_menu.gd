extends Node

@export var ip_entry: LineEdit
@export var server_port_entry: LineEdit

@export var code_entry: LineEdit

func _ready():
	Lobby.connected_and_registered.connect(_on_connection_successful)
	Lobby.server_disconnected.connect(_on_server_disconnected)

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


func _on_btn_quit_pressed() -> void:
	get_tree().quit()

func _on_btn_server_pressed():
	if multiplayer.multiplayer_peer:
		multiplayer.multiplayer_peer.close()
	# Ensure IP and Port are valid
	if not check_entries():
		return
	Lobby.create_game()

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
	var error = Lobby.join_game(ip_entry.text, int(server_port_entry.text))
	if error:
		error_dialog("Invalid Connection", "Could not connect. Error: " + str(error))
		return


func _on_connection_successful():
	if get_tree():
		get_tree().change_scene_to_file("res://scenes/menu/lobby_menu.tscn")

func _on_server_disconnected():
	error_dialog("Disconnected", "Unable to join the server. Are you using the same game version?")




func _on_advanced_label_meta_clicked(meta):
	$ServerControls/AdvancedSettings.visible = not $ServerControls/AdvancedSettings.visible
	# Clear the entries
	ip_entry.text = ""
	server_port_entry.text = ""
