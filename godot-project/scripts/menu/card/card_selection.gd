extends Control

var card_scene: PackedScene = preload("res://scenes/game/card/card.tscn")

var showing_cards: bool = false
var last_hovered: int = -1

signal card_selected(card_id: int)
signal cards_showing()

var cards: Array[Control] = []

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
	for child in %CardContainer.get_children():
		# If card is already at y 0, ignore
		if child.position.y == 0:
			continue
		var tween: Tween = create_tween()
		tween.tween_callback(func(): child.position.y = 500)
		tween.tween_interval(0.5 * (child_num+1))
		tween.set_ease(Tween.EASE_OUT)
		tween.set_trans(Tween.TRANS_CUBIC)
		tween.tween_property(child, "position:y", 0.0, 0.8)
		last_tween = tween
		child_num += 1
	
	if last_tween != null:
		last_tween.tween_callback(_cards_showing)
	else:
		showing_cards = false

func clear_cards() -> void:
	for child in %CardContainer.get_children():
		child.queue_free()

func add_card(card_data: Dictionary) -> void:
	var card = card_scene.instantiate()
	if card_data.has("name"):
		card.set_card_name(card_data["name"])
	if card_data.has("image_loc"):
		card.set_card_image(card_data["image_loc"])
	if card_data.has("description"):
		card.set_card_description(card_data["description"])
	if card_data.has("card_id"):
		card.card_id = card_data["card_id"]
	%CardContainer.add_child(card)
	cards.append(card)

func _cards_showing() -> void:
	cards_showing.emit()
	enable_cards()

func enable_cards() -> void:
	showing_cards = false
	for child in %CardContainer.get_children():
		child.set_enabled(true)

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	if Input.is_action_just_pressed("mouse_left"):
		if last_hovered != -1:
			card_selected.emit(last_hovered)
			print("Selected: %s" % last_hovered)
	if Input.is_action_just_pressed("mouse_right"):
		hide_cards()
		show_cards()

func _hovered(card_id: int) -> void:
	last_hovered = card_id

func _unhovered(card_id: int) -> void:
	if last_hovered == card_id:
		last_hovered = -1
