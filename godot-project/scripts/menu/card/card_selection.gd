extends Control

var card_scene: PackedScene = preload("res://scenes/game/card/card.tscn")

var showing_cards: bool = false
var last_hovered: int = -1

signal card_selected(card_id: int)
signal cards_showing()

var cards: Array[Control] = []

var active_tweens: Array = []

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	for child in %CardContainer.get_children():
		child.hover.connect(_hovered)
		child.unhover.connect(_unhovered)

func _enter_tree() -> void:
	# On first frame the cards are shown, hide the cards
	await get_tree().process_frame
	hide_cards()

func hide_cards() -> void:
	if showing_cards:
		await cards_showing
	for child in %CardContainer.get_children():
		child.set_enabled(false)
		child.reset_offset()
		child.position.y = 500

func show_cards() -> void:
	if showing_cards:
		return
	
	showing_cards = true
	last_hovered = -1
	
	var child_num: int = 0
	var last_tween: Tween
	
	for card in cards:
		# If card is already at y 0, ignore
		card._hold_up = false
		card.hover_enabled = false
		card.set_enabled(false)
		card.card_panel.visible = false
		
		var tween: Tween = create_tween()
		active_tweens.append(tween)
		tween.tween_callback(func():
			card.position.y = 500
		)
		tween.tween_interval(0.5 * (child_num+1))
		tween.tween_callback(func():
			card.position.y = 500
			card.card_panel.visible = true
		)
		tween.set_ease(Tween.EASE_OUT)
		tween.set_trans(Tween.TRANS_CUBIC)
		tween.tween_property(card, "position:y", 0.0, 0.8)
		tween.tween_callback(func():
			card.hover_enabled = true
			card.set_enabled(true)
			active_tweens.erase(tween)
		)
		last_tween = tween
		child_num += 1
	
	if last_tween != null:
		last_tween.tween_callback(_cards_showing)
	else:
		showing_cards = false

func clear_cards() -> void:
	# Kill all tweens
	for tween in active_tweens:
		tween.kill()
	active_tweens.clear()
	# Free all children
	for card in cards:
		card.queue_free()
	cards.clear()

func add_card(card_data: Dictionary) -> void:
	var card = card_scene.instantiate()
	%CardContainer.add_child(card)
	if card_data.has("name"):
		card.set_card_name(card_data["name"])
	if card_data.has("image_loc"):
		card.set_card_image(card_data["image_loc"])
	if card_data.has("description"):
		card.set_card_description(card_data["description"])
		card._update_desc_scroll()
	if card_data.has("card_id"):
		card.card_id = card_data["card_id"]
	card.hover.connect(_hovered)
	card.unhover.connect(_unhovered)
	cards.append(card)

func get_card(card_id: int) -> Node:
	for card in cards:
		if card.card_id == card_id:
			return card
	return null

func put_card(card_id: int, up: bool) -> void:
	var card = get_card(card_id)
	# Ignore if no matching card
	if card == null:
		return
	
	card.hold(up)


func _cards_showing() -> void:
	cards_showing.emit()
	enable_cards()

func enable_cards() -> void:
	showing_cards = false
	for card in cards:
		card.set_enabled(true)

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	if Input.is_action_just_pressed("mouse_left"):
		if last_hovered != -1:
			card_selected.emit(last_hovered)

func _hovered(card: Node) -> void:
	last_hovered = card.card_id

func _unhovered(card: Node) -> void:
	if last_hovered == card.card_id:
		last_hovered = -1

func _notification(what: int) -> void:
	if what == NOTIFICATION_PREDELETE:
		clear_cards()
