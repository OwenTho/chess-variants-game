extends Control

@export var hover_time: float = 0.3
@export var hover_offset: float = -20
@export_range(1,30,1,"or_greater") var default_font_size: int = 24

var cur_tween: Tween

var card_id: int = -1

var _hold_up: bool = false
var _hover: bool = false
var currently_up: bool = false
@onready var card_panel: PanelContainer = $CardOffset/CardPanel

signal hover(card_id: int)
signal unhover(card_id: int)


func set_enabled(enable: bool) -> void:
	if enable:
		$CardOffset/CardPanel.mouse_filter = MOUSE_FILTER_PASS
	else:
		$CardOffset/CardPanel.mouse_filter = MOUSE_FILTER_IGNORE

func reset_offset() -> void:
	if cur_tween != null:
		cur_tween.stop()
	_hover = false
	_hold_up = false
	currently_up = false
	$CardOffset/CardPanel.position.y = 0


func set_card_name(name: String) -> void:
	%NameLabel.text = name
	var font: Font = (%NameLabel as Label).get_theme_default_font()
	var font_size: int = default_font_size
	
	var label_settings: LabelSettings = %NameLabel.label_settings
	# Until the text fits the label, make the text smaller
	# Font size has a minimum of 1
	while font.get_string_size(name, HORIZONTAL_ALIGNMENT_CENTER, -1, font_size).x > 190 and font_size >= 1:
		font_size -= 1
	label_settings.font_size = max(font_size, 1)

func set_card_image(image_loc: String) -> void:
	# Try to get the image
	var card_texture: Texture
	if ResourceLoader.exists("res://" + image_loc):
		card_texture = load("res://" + image_loc)
	else:
		push_warning("Could not find sprite at path '%s', so defalt is being used." % [image_loc])
		card_texture = load("res://assets/texture/card/missing.png")
	
	(%CardImage as TextureRect).texture = card_texture

func set_card_description(description: String) -> void:
	%DescriptionLabel.text = description

func _tween_end() -> void:
	cur_tween = null

func hold(up: bool) -> void:
	# If not help up yet, hold up
	if not currently_up and up:
		move_up()
	# If held up, but it shouldn't be, put down
	elif currently_up and not up:
		_hold_up = false
		move_down()
	_hold_up = up

func _stop_tween() -> void:
	if cur_tween != null:
		cur_tween.stop()
		cur_tween = null

func move_up() -> void:
	# If already help up, don't hold up
	if currently_up:
		currently_up = true
		return
	_stop_tween()
	cur_tween = create_tween()
	cur_tween.tween_property(card_panel, "position:y", hover_offset, hover_time)
	cur_tween.tween_callback(_tween_end)
	currently_up = true

func move_down() -> void:
	# If already down, don't put down
	if _hold_up:
		return
	if not currently_up:
		currently_up = false
		return
	_stop_tween()
	cur_tween = create_tween()
	cur_tween.tween_property(card_panel, "position:y", 0.0, hover_time)
	cur_tween.tween_callback(_tween_end)
	currently_up = false

func _on_panel_container_mouse_entered() -> void:
	hover.emit(card_id)

	move_up()
	_hover = true

func _on_panel_container_mouse_exited() -> void:
	unhover.emit(card_id)
	
	move_down()
	_hover = false
