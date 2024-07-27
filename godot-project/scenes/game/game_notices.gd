extends VBoxContainer

@export var max_notices: int = 5

func add_notice(text: String) -> void:
	var new_label: Label = Label.new()
	new_label.text = text
	add_child(new_label)
	new_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	get_tree().create_timer(5).timeout.connect(remove_notice.bind(new_label))
	if get_child_count() > max_notices:
		get_child(0).queue_free()

func remove_notice(label: Label) -> void:
	var new_tween = create_tween()
	new_tween.tween_property(label, "modulate", Color(255, 255, 255, 0), 0.5)
	new_tween.tween_callback(label.queue_free)
