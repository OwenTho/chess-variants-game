extends Control

@export var player_name_scene: PackedScene
@export var name_container: Control
@export var name_edit: LineEdit
@export var name_timer: Timer

@export var message_holder: VBoxContainer
@export var message_entry: TextEdit
@export var scroll_container: ScrollContainer

@export var lobby_code: Label

var max_scroll: int = 0

var player_labels: Dictionary = {}

var player_nums: Array[int] = [-1,-1]

func _ready():
	Lobby.player_connected.connect(_on_player_registered)
	Lobby.player_disconnected.connect(_on_player_disconnected)
	Lobby.player_data_received.connect(_on_data_received)
	Lobby.player_num_updated.connect(_on_player_nums_changed)
	Lobby.server_disconnected.connect(close_lobby)
	
	Lobby.received_message.connect(_on_received_message)
	
	var scroll_bar = scroll_container.get_v_scroll_bar()
	scroll_bar.changed.connect(scroll_to_bottom)
	max_scroll = scroll_bar.max_value
	
	name_edit.max_length = Lobby.NAME_LENGTH_LIMIT
	
	lobby_code.text = Lobby.code.to_upper()
	
	# Display all data currently obtained. This should be
	# the people in the lobby before this player joined
	for id in Lobby.players:
		_on_player_registered(Lobby.players[id])
	
	for i in range(Lobby.player_nums.size()):
		_on_player_nums_changed(Lobby.player_nums[i], i)
	
	if not is_multiplayer_authority() and not Lobby.player_info.is_admin:
		$BtnPlay.disabled = true
	
	# Set NameEdit to this player's name
	name_edit.text = Lobby.players[multiplayer.get_unique_id()]["name"]

func _make_label(id: int) -> PlayerNameContainer:
	var new_label: PlayerNameContainer = player_name_scene.instantiate()
	new_label.player_id = id
	new_label.player_button_pressed.connect(_on_player_button_pressed)
	name_container.add_child(new_label)
	if id <= -1:
		new_label.set_text("Invalid ID")
	player_labels[id] = new_label
	return new_label

func _update_label(id: int, text: String):
	if not player_labels.has(id):
		var new_label: PlayerNameContainer = _make_label(id)
	player_labels[id].set_text(text)

func _on_player_registered(player_data: Lobby.PlayerInfo):
	_update_label(player_data.id, player_data.name)

func _on_player_disconnected(id: int):
	if not player_labels.has(id):
		_make_label(id)
	var player_label: PlayerNameContainer = player_labels[id]
	player_labels.erase(id)
	player_label.queue_free()

func _on_data_received(player_data: Lobby.PlayerInfo):
	var player_label: PlayerNameContainer = player_labels[player_data.id]
	_update_label(player_data.id, player_data.name)
	if player_data.id == multiplayer.get_unique_id() and player_data.name != name_edit.text:
		name_edit.text = player_data.name

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

func _on_name_edit_text_changed(text: String):
	if text.is_empty():
		return
	name_timer.start()


func _on_btn_leave_pressed():
	Lobby.leave_game()
	close_lobby()
	
func _on_player_button_pressed(id: int, player_num: int):
	Lobby.request_set_player.rpc(id, player_num)


func _on_name_change_timer_timeout():
	Lobby.change_name.rpc(name_edit.text)


func _on_name_edit_focus_exited():
	name_timer.stop()
	# Check to make sure multiplayer peer is still there
	if multiplayer.multiplayer_peer:
		Lobby.change_name.rpc(name_edit.text)


func _on_btn_play_pressed():
	Lobby.request_start_game.rpc()


var caret_col: int = 0

func _on_text_edit_text_changed():
	var changed: bool = false
	if message_entry.text.contains("\n"):
		message_entry.text = message_entry.text.replace("\n", "")
		changed = true
	
	if message_entry.text.length() > Lobby.MESSAGE_LIMIT:
		message_entry.text = message_entry.text.substr(0, Lobby.MESSAGE_LIMIT)
		changed = true
	
	
	if changed:
		message_entry.set_caret_column(caret_col)

func _on_text_edit_caret_changed():
	if message_entry.get_caret_column() == caret_col:
		return
	caret_col = message_entry.get_caret_column()

func _input(event):
	if message_entry.has_focus():
		if event is InputEventKey and event.is_pressed():
			if event.key_label == KEY_ENTER:
				_on_btn_send_pressed()

func _on_btn_send_pressed():
	# Send request to server
	Lobby.send_message.rpc(message_entry.text)
	# Then clear text
	message_entry.text = ""

func scroll_to_bottom():
	var scroll_bar = scroll_container.get_v_scroll_bar()
	if max_scroll != scroll_bar.max_value:
		# If at the previous max value, scroll
		scroll_container.scroll_vertical = scroll_bar.max_value
		max_scroll = scroll_bar.max_value

func _on_received_message(message: String):
	# Make a new label
	var new_label: RichTextLabel = RichTextLabel.new()
	new_label.text = message
	new_label.fit_content = true
	new_label.bbcode_enabled = true
	new_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	message_holder.add_child(new_label)
	
	
	# If there are > 30 messages, remove the last one
	if message_holder.get_child_count(false) > 30:
		message_holder.get_child(0).queue_free()

func _on_btn_clear_pressed():
	for child in message_holder.get_children():
		child.queue_free()
