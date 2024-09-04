class_name GameAddon
extends Node

var _card_notices: Dictionary = {}

func add_card_notice(notice: String, function: Callable) -> void:
	if notice not in _card_notices:
		_card_notices[notice] = []
	_card_notices[notice].append(function)

func get_cs_scripts() -> Array[CSharpScript]:
	return []

func _handle_card_notice(card: Node, notice: String) -> void:
	if notice not in _card_notices:
		return
	var listeners: Array = _card_notices[notice]
	for listener in listeners:
		if listener is not Callable:
			push_error("Non-Callable listner found in card_notices: %s" % [listener])
			continue
		listener.call(card)
