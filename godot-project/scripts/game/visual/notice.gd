extends Label

# Timer should autostart
@export var removal_timer: Timer

# When timer is done, disappear and free
func _on_remove_timer_timeout() -> void:
	var new_tween = create_tween()
	new_tween.tween_property(self, "modulate", Color(255, 255, 255, 0), 0.5)
	new_tween.tween_callback(queue_free)
