extends Control

signal card_selected(card: Node)

@export var length: float = 350

@export var max_scale: float = 0.3
@export var min_scale: float = 0.2

@export var max_dist: float = 100
@export var min_dist: float = 10

var _cards: Array[Node]
var _tweens: Array[Array]

var cur_hover: Node

func add_card(card: Node) -> bool:
	if _cards.has(card):
		return false
	card.card_id = _cards.size()
	_cards.append(card)
	card.input.connect(_on_card_gui_input)
	card.hover.connect(_on_card_hover)
	card.unhover.connect(_on_card_unhover)
	if card.get_parent() != null:
		card.reparent(self)
	else:
		add_child(card)
	return true

func remove_card(card: Node) -> bool:
	if not _cards.has(card):
		return false
	card.card_id = -1
	_cards.erase(card)
	card.input.disconnect(_on_card_gui_input)
	card.hover.disconnect(_on_card_hover)
	card.unhover.disconnect(_on_card_unhover)
	return true

func _calc_card_distance() -> float:
	return min(length / _cards.size(), max_dist)

func _calc_card_scale() -> float:
	return max_scale

func _calc_card_pos(card_id: int, card_distance: float, card_scale: float) -> Vector2:
	var mid_ind = _cards.size() / 2.0
	var rel_ind: float = card_id + 0.5 - mid_ind
	var target_rel_pos: float = rel_ind * card_distance
	var target_final_pos: Vector2 = Vector2(target_rel_pos, 0)
	target_final_pos += size / 2
	return target_final_pos

func _move_card(card: Node, pos: Vector2, final_scale: Vector2) -> Tween:
	
	card.set_enabled(false)
	card._hover = false
	card.currently_up = false
	card._hold_up = false
	
	var card_tween: Tween = create_tween()
	card_tween.set_ease(Tween.EASE_OUT)
	card_tween.set_trans(Tween.TRANS_CUBIC)
	card_tween.tween_property(card, "position", pos, 0.5)
	card_tween.parallel().tween_property(card, "scale", final_scale, 0.5)
	card_tween.finished.connect(_on_tween_end.bind(card_tween))
	card_tween.finished.connect(_on_card_move_end.bind(card))
	return card_tween

func move_card(card_id: int) -> void:
	if card_id < 0 or card_id >= _cards.size():
		push_error("No card of id %s" % [card_id])
		return
	
	var card = _cards[card_id]
	# Kill existing tweens
	for tween in _tweens:
		if tween[0] == card:
			tween[1].kill()
			_on_tween_end(tween[1])
	
	# Now move the card
	var card_distance = _calc_card_distance()
	var card_scale = _calc_card_scale()
	
	var target_final_pos: Vector2 = _calc_card_pos(card_id, card_distance, card_scale)
	var tween = _move_card(card, target_final_pos, Vector2(card_scale, card_scale))
	_tweens.append([card, tween])

func move_cards() -> void:
	# Reset existing tweens
	for tween in _tweens:
		tween[1].kill()
	_tweens.clear()
	# First calculate the distance + scale values
	
	var card_distance = _calc_card_distance()
	var card_scale = _calc_card_scale()
	
	var mid_ind: float = _cards.size() / 2.0
	for card_ind in range(_cards.size()):
		var card: Node = _cards[card_ind]
		var target_final_pos: Vector2 = _calc_card_pos(card_ind, card_distance, card_scale)
		var tween = _move_card(card, target_final_pos, Vector2(card_scale, card_scale))
		_tweens.append([card, tween])

func _on_tween_end(tween: Tween) -> void:
	for i in range(_tweens.size()):
		if _tweens[i][1] == tween:
			_tweens.remove_at(i)
			break

func _on_card_move_end(card: Node) -> void:
	card.set_enabled(true)

func _on_card_gui_input(card: Node, event: InputEvent) -> void:
	if card != cur_hover:
		return
	
	if event is not InputEventMouseButton:
		return
	
	if event.is_action_pressed("mouse_left"):
		card_selected.emit(card)

func _on_card_hover(card: Node) -> void:
	cur_hover = card

func _on_card_unhover(card: Node) -> void:
	if cur_hover == card:
		cur_hover = null
