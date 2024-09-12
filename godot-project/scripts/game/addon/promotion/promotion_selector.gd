extends Node2D

@export var promotion_container: HBoxContainer

var promotion_option_scene: PackedScene = preload("res://scenes/game/addons/promotion/promotion_option.tscn")
var promotion_options: Array = []
var mat: Material = null

signal selection_made(piece_info)

func add_option(piece_info, id: int) -> void:
	var new_promotion_scene: Button = promotion_option_scene.instantiate()
	
	new_promotion_scene.piece_info = piece_info
	new_promotion_scene.id = id
	new_promotion_scene.update_texture()
	new_promotion_scene.material = mat
	
	promotion_container.add_child(new_promotion_scene)
	
	# Add listener
	new_promotion_scene.selection_made.connect(_on_selection_made)
	
	promotion_options.append(promotion_options)

func set_option_material(mat: Material) -> void:
	self.mat = mat
	for option in promotion_options:
		option.material = mat

func clear_options() -> void:
	for option in promotion_options:
		option.on_option_pressed.disconnect(_on_selection_made)
		option.queue_free()
	promotion_options.clear()

func _on_selection_made(option: Button) -> void:
	selection_made.emit(option.piece_info, option.id)
