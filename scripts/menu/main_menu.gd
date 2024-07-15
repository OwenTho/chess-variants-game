extends Node


func _on_btn_play_pressed() -> void:
	get_tree().change_scene_to_file("res://scenes/game/game_screen.tscn")


func _on_btn_quit_pressed() -> void:
	get_tree().quit()

