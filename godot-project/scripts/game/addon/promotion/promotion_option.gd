extends Control

@export var texture_rect: TextureRect

var piece_info
var id: int

signal selection_made(this_node: TextureRect)

func update_texture() -> void:
	texture_rect.texture = GameResources.get_piece_texture_from_info(piece_info)

func _on_pressed() -> void:
	selection_made.emit(self)
