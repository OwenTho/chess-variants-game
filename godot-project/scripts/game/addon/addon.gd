class_name GameAddon
extends Node

var _card_notices: Dictionary = {}

func add_card_notice(card_ids, notice: String, function: Callable) -> void:
	var valid_id: bool = false
	if card_ids != null:
		if card_ids is String:
			card_ids = [card_ids]
			valid_id = true
		elif card_ids is Array:
			valid_id = true
			for val in card_ids:
				if val is not String:
					valid_id = false
					break
	else:
		card_ids = []
		valid_id = true
	
	if not valid_id:
		push_error("card_ids must be a String, an Array of Strings or null.")
		return
	
	if notice not in _card_notices:
		_card_notices[notice] = []
	_card_notices[notice].append({"card_ids": card_ids, "callback": function})

func add_any_card_notice(notice: String, function: Callable) -> void:
	add_card_notice(null, notice, function)

func get_cs_scripts() -> Array[CSharpScript]:
	return []

func _handle_card_notice(card: Node, notice: String) -> void:
	if notice not in _card_notices:
		return
	var listeners: Array = _card_notices[notice]
	for listener in listeners:
		if "card_ids" not in listener:
			push_error("'card_ids' missing from listener.")
			continue
		if listener["card_ids"] is not Array:
			push_error("Card Ids in listeners should be stored as an Array.")
			continue
		var card_ids: Array = listener["card_ids"]
		if "callback" not in listener:
			push_error("'callback' missing from listener.")
			continue
		if listener["callback"] is not Callable:
			push_error("Non-Callable listner found in card_notices: %s" % [listener])
			continue
		
		# Card ID must match
		if not card_ids.is_empty() and not listener["card_ids"].has(card.cardId):
			continue
		listener["callback"].call(card)
