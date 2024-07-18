extends Node2D


# Called when the node enters the scene tree for the first time.
func _ready():
	# Check if this is a server
	if OS.has_feature("dedicated_server"):
		# If it's a dedicated server, immediately go and make a lobby
		Lobby.create_game()
		return
	
	# If it's not a server, go to the main menu
	get_tree().change_scene_to_file.call_deferred("res://scenes/menu/main_menu.tscn")
