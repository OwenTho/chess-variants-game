extends Node2D


func _ready():
	# Check if default port was provided
	var command_args: PackedStringArray = OS.get_cmdline_args()
	# Loop for specific arguments
	for arg in command_args:
		print(arg)
		# Lower case
		arg = arg.to_lower()
		# If it does not start with "--", ignore
		if not arg.begins_with("--"):
			continue
		arg = arg.substr(2)
		# If it's missing a "=", continue
		if not arg.count("=") == 1:
			process_argument(arg)
			continue
		var split_arg: PackedStringArray = arg.split("=", true, 1)
		process_argument(split_arg[0], split_arg[1])
		
	# Check if this is a server
	if OS.has_feature("dedicated_server") or OS.get_cmdline_args().has("--server"):
		print("Running as server.")
		Lobby.is_player = false
		Lobby.close_on_noone = true
		# If it's a dedicated server, immediately go and make a lobby
		Lobby.create_game()
		return
	Lobby.is_player = true
	get_tree().root.title = "Chess Variants Game"
	print("Running as client.")
	# If it's not a server, go to the main menu
	get_tree().change_scene_to_file.call_deferred("res://scenes/menu/main_menu.tscn")



func process_argument(name: String, value: String = ""):
		match name:
			### Parameters
			# Close when there are no players left
			"close-on-noone":
				Lobby.close_on_noone = true
			
			
			### Parameters with Arguments
			## Maximum server connections
			"name-limit":
				if not value.is_valid_int():
					push_error("Name limit must be an int. Leaving at default.")
					return
				var name_limit: int = int(value)
				if name_limit <= 3:
					push_error("Default Name is 'Name', so limit must be 4 or more. Leaving at default.")
					return
				Lobby.NAME_LENGTH_LIMIT = name_limit
			"max-connections":
				if not value.is_valid_int():
					push_error("Maximum connections must be an int.")
					get_tree().quit()
					return
				var connections: int = int(value)
				if connections < 2:
					push_error("Must allow at least 2 connections.")
					get_tree().quit()
					return
				Lobby.MAX_CONNECTIONS = connections
			## Default ip to use in Lobby
			# Used for Clients
			"default-ip":
				if not value.is_valid_ip_address():
					push_error("Invalid IP: " % value)
					get_tree().quit()
				Lobby.DEFAULT_SERVER_IP = value
			## Default port to use in Lobby
			# Fails if incorrect, for servers
			# Servers use this for their Port.
			"default-port":
				if not value.is_valid_int():
					push_error("Port must be an integer.")
					get_tree().quit()
					return
				if not value.length() < 6:
					push_error("Port must have at most 5 characters. 1024 - 49151.")
					get_tree().quit()
					return
				var port_number: int = int(value)
				# If in invalid range, quit
				if port_number < 0 or port_number > 64738:
					push_error("Invalid Port (%s). Use a port in the range 1024 - 49151." % port_number)
					get_tree().quit()
					return
				# If it's in claimed range, warn
				if port_number < 1024:
					push_error("Port %s is a commonly used port. Use a port in the range 1024 - 49151." % port_number)
					get_tree().quit()
					return
				elif port_number > 49151:
					push_error("Port %s is a ephermal port. Use a port in the range 1024 - 49151." % port_number)
					get_tree().quit()
					return
				# If port is valid, assign to the port
				Lobby.DEFAULT_PORT = port_number
				print("Default port has been set to %s." % [port_number])
			## The code of the lobby
			"lobby-code":
				Lobby.code = value
				print("Lobby code is %s" % value)
			_:
				print("Unknown argument \"%s\"" % [name])
