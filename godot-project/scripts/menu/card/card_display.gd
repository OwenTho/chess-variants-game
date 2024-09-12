extends Control

@export var length: float = 350

@export var max_scale: float = 0.3
@export var min_scale: float = 0.2

@export var max_dist: float = 100
@export var min_dist: float = 10

var _cards: Array[Node]
var _tweens: Array[Tween]

func add_card(card: Node) -> bool:
	if _cards.has(card):
		return false
	_cards.append(card)
	if card.get_parent() != null:
		card.reparent(self)
	else:
		add_child(card)
	return true

func remove_card(card: Node) -> bool:
	if _cards.has(card):
		_cards.erase(card)
		return true
	return false

func move_cards() -> void:
	# Reset existing tweens
	for tween in _tweens:
		tween.kill()
	_tweens.clear()
	# First calculate the distance + scale values
	
	var card_distance = min(length / _cards.size(), max_dist)
	var card_scale = max_scale
	
	var mid_ind: float = _cards.size() / 2.0
	for card_ind in range(_cards.size()):
		var card: Node = _cards[card_ind]
		var rel_ind: float = card_ind + 0.5 - mid_ind
		var target_rel_pos: float = rel_ind * card_distance
		var target_final_pos: Vector2 = Vector2(target_rel_pos, 0)
		target_final_pos += size / 2
		
		card.set_enabled(false)
		card._hover = false
		card.currently_up = false
		card._hold_up = false
		
		var card_tween: Tween = create_tween()
		card_tween.set_ease(Tween.EASE_OUT)
		card_tween.set_trans(Tween.TRANS_CUBIC)
		card_tween.tween_property(card, "position", target_final_pos, 0.5)
		card_tween.parallel().tween_property(card, "scale", Vector2(card_scale, card_scale), 0.5)
		card_tween.finished.connect(_on_card_move_end.bind(card))

func _on_card_move_end(card: Node) -> void:
	card.set_enabled(true)
