extends Node2D


func _ready():
	# Check if this is a server
	if OS.has_feature("dedicated_server"):
		print("Running as server.")
		Lobby.is_player = false
		# If it's a dedicated server, immediately go and make a lobby
		Lobby.create_game()
		return
	Lobby.is_player = true
	get_tree().root.title = "Chess Variants Game"
	print("Running as client.")
	# If it's not a server, go to the main menu
	get_tree().change_scene_to_file.call_deferred("res://scenes/menu/main_menu.tscn")
