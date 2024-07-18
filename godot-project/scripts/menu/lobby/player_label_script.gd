extends HSplitContainer

class_name PlayerNameContainer

signal player_button_pressed(id: int, player_num: int)

var player_id: int = -1

@onready var player_num_label: Label = $HBoxContainer/HSplitContainer/PlayerLabel

func _ready():
	if not is_multiplayer_authority():
		$HBoxContainer/HSplitContainer/HBoxContainer/BtnPlayer1.disabled = true
		$HBoxContainer/HSplitContainer/HBoxContainer/BtnPlayer2.disabled = true

func set_text(name: String):
	$PlayerNameLabel.text = name

func set_player(num: int):
	if num <= -1:
		player_num_label.text = ""
		return
	player_num_label.text = "P" + str(num + 1)

func _on_btn_player_1_pressed():
	player_button_pressed.emit(player_id, 0)


func _on_btn_player_2_pressed():
	player_button_pressed.emit(player_id, 1)


