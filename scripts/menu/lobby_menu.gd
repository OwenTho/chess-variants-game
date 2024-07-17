extends Control

@export var player_name_scene: PackedScene
@export var name_container: Control
@export var name_edit: LineEdit
@export var name_timer: Timer

var player_labels: Dictionary = {}

var player_nums: Array[int] = [-1,-1]

func _ready():
	Lobby.player_connected.connect(_on_player_connected)
	Lobby.player_disconnected.connect(_on_player_disconnected)
	Lobby.player_data_received.connect(_on_data_received)
	Lobby.server_disconnected.connect(_on_server_disconnected)
	Lobby.connection_failed.connect(_on_server_disconnected)
	Lobby.player_num_updated.connect(_on_player_nums_changed)
	
	name_edit.max_length = Lobby.NAME_LENGTH_LIMIT
	
	# Use currently obtained data
	for id in Lobby.players:
		_on_player_connected(id, Lobby.players[id])
	
	for i in range(Lobby.player_nums.size()):
		_on_player_nums_changed(Lobby.player_nums[i], i)
	
	if not is_multiplayer_authority():
		$BtnPlay.disabled = true
	
	# Set NameEdit to this player's name
	name_edit.text = Lobby.players[multiplayer.get_unique_id()]["name"]

func _make_label(id: int) -> PlayerNameContainer:
	var new_label: PlayerNameContainer = player_name_scene.instantiate()
	new_label.player_id = id
	new_label.player_button_pressed.connect(_on_player_button_pressed)
	name_container.add_child(new_label)
	player_labels[id] = new_label
	return new_label

func _on_player_connected(id: int, player_data: Dictionary):
	if not player_labels.has(id):
		var new_label: PlayerNameContainer = _make_label(id)
	player_labels[id].set_text(player_data["name"])

func _on_player_disconnected(id: int):
	if not player_labels.has(id):
		_make_label(id)
	var player_label: PlayerNameContainer = player_labels[id]
	player_labels.erase(id)
	player_label.queue_free()

func _on_data_received(id: int, player_data: Dictionary):
	var player_label: PlayerNameContainer = player_labels[id]
	player_label.set_text(player_data["name"])
	if id == multiplayer.get_unique_id() and player_data["name"] != name_edit.text:
		name_edit.text = player_data["name"]

func _on_player_nums_changed(id: int, player_num: int):
	var old_id: int = player_nums[player_num]
	if old_id >= 0:
		# If the label is there, set player
		if player_labels.has(old_id):
			player_labels[old_id].set_player(-1)
	player_nums[player_num] = id
	if id == -1:
		return
	if not player_labels.has(id):
		_make_label(id)
	player_labels[id].set_player(player_num)

func close_lobby():
	get_tree().change_scene_to_file("res://scenes/menu/main_menu.tscn")

func _on_server_disconnected():
	close_lobby()

func _on_name_edit_text_changed(text: String):
	if text.is_empty():
		return
	name_timer.start()
	#var cursor_col = name_edit.get_caret_column()
	#var cursor_line = name_edit.get_caret_line()
	
#	if text.length() > Lobby.NAME_LENGTH_LIMIT:
#		text = text.substr(0,Lobby.NAME_LENGTH_LIMIT)
#		name_edit.text = text
#		name_edit.set_caret_column(cursor_col)
#		name_edit.set_caret_line(cursor_line)
	


func _on_btn_leave_pressed():
	Lobby.leave_game()
	close_lobby()
	
func _on_player_button_pressed(id: int, player_num: int):
	Lobby.set_player(id, player_num)


func _on_name_change_timer_timeout():
	Lobby.change_name.rpc(name_edit.text)


func _on_name_edit_focus_exited():
	name_timer.stop()
	Lobby.change_name.rpc(name_edit.text)


func _on_btn_play_pressed():
	if not is_multiplayer_authority():
		return
	Lobby.start_game()
