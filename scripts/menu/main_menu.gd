extends Node

func _ready():
	Lobby.connection_successful.connect(_on_connection_successful)

func _on_btn_play_pressed() -> void:
	get_tree().change_scene_to_file("res://scenes/game/game_screen.tscn")


func _on_btn_quit_pressed() -> void:
	get_tree().quit()

func _on_btn_server_pressed():
	Lobby.create_game()

func _on_btn_join_pressed():
	Lobby.join_game()



func _on_connection_successful():
	get_tree().change_scene_to_file("res://scenes/menu/lobby_menu.tscn")



