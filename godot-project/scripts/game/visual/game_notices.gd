extends VBoxContainer

@export var max_notices: int = 5

var notice_scene: PackedScene = preload("res://scenes/game/visual/notice.tscn")

func add_notice(text: String) -> void:
	var new_label: Label = notice_scene.instantiate()
	new_label.text = text
	add_child(new_label)
	if get_child_count() > max_notices:
		get_child(0).queue_free()

func remove_notice(label: Label) -> void:
	var new_tween = create_tween()
	new_tween.tween_property(label, "modulate", Color(255, 255, 255, 0), 0.5)
	new_tween.tween_callback(label.queue_free)
