extends Node

@export var ip_entry: LineEdit

func _ready():
	Lobby.connection_successful.connect(_on_connection_successful)

func _on_btn_play_pressed() -> void:
	# Disconnect signal to avoid loading lobby menu
	Lobby.connection_successful.disconnect(_on_connection_successful)
	# Start a local server
	Lobby.create_game(false)
	# Set all players to the server
	for i in range(Lobby.player_nums.size()):
		Lobby.player_nums[i] = 1
	# Start the game
	Lobby.start_game()


func _on_btn_quit_pressed() -> void:
	get_tree().quit()

func _on_btn_server_pressed():
	Lobby.create_game()

func _on_btn_join_pressed():
	var join_ip: String = ip_entry.text
	if not join_ip.is_empty() and not join_ip.is_valid_ip_address():
		var new_dialog = AcceptDialog.new()
		new_dialog.title = "Invalid IP"
		new_dialog.dialog_text = "Please enter a valid IP."
		new_dialog.close_requested.connect(new_dialog.queue_free)
		add_child(new_dialog)
		new_dialog.popup_centered()
		return
	
	var error = Lobby.join_game(join_ip)
	if error:
		print(error)
		var new_dialog = AcceptDialog.new()
		new_dialog.title = "Invalid Connection"
		new_dialog.dialog_text = "Could not connect. Error Code: " + str(error)
		new_dialog.close_requested.connect(new_dialog.queue_free)
		add_child(new_dialog)
		new_dialog.popup_centered()
		return



func _on_connection_successful():
	get_tree().change_scene_to_file("res://scenes/menu/lobby_menu.tscn")



