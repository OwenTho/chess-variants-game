extends Button

func _ready() -> void:
	if not OS.is_debug_build():
		visible = false
		queue_free()

func _on_pressed() -> void:
	print_orphan_nodes()
