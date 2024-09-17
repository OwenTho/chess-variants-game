extends Control

signal input(card: Node, event: InputEvent)
signal hover(card: Node)
signal unhover(card: Node)

@export var hover_time: float = 0.3
@export var hover_offset: float = -20
@export_range(1,30,1,"or_greater") var default_font_size: int = 24

var cur_tween: Tween

var card_id: int = -1

var _hold_up: bool = false
var _hover: bool = false
var currently_up: bool = false
var hover_enabled: bool = true
@onready var card_panel: PanelContainer = $CardOffset/CardPanel

func set_enabled(enable: bool) -> void:
	if enable:
		$CardOffset/CardPanel.mouse_filter = MOUSE_FILTER_PASS
	else:
		$CardOffset/CardPanel.mouse_filter = MOUSE_FILTER_IGNORE

func reset_offset() -> void:
	if cur_tween != null:
		cur_tween.kill()
		cur_tween = null
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
		push_warning("Could not find sprite at path '%s', so default is being used." % [image_loc])
		card_texture = load("res://assets/texture/card/missing.png")
	
	(%CardImage as TextureRect).texture = card_texture

var desc_scroll_tween: Tween = null
func _update_desc_scroll() -> void:
	
	if desc_scroll_tween != null:
		desc_scroll_tween.kill()
		desc_scroll_tween = null
	
	# If there's a scroll bar, disable it and make it automated
	var scroll: VScrollBar = %DescriptionLabel.get_v_scroll_bar()
	var line_count: int = %DescriptionLabel.get_line_count()
	if scroll != null:
		scroll.mouse_filter = Control.MOUSE_FILTER_IGNORE
		if line_count < 7:
			return
		await get_tree().process_frame
		var scroll_time: float = (line_count-6.)*0.5
		scroll.step = 0.01
		desc_scroll_tween = create_tween().set_loops()
		desc_scroll_tween.tween_interval(2)
		desc_scroll_tween.tween_property(scroll, "value", %DescriptionLabel.get_content_height()-%DescriptionLabel.size.y, scroll_time)
		desc_scroll_tween.tween_interval(2)
		desc_scroll_tween.tween_property(scroll, "value", 0.0, scroll_time)

func enable_desc_scroll(enabled: bool = true):
	var scroll: VScrollBar = %DescriptionLabel.get_v_scroll_bar()
	if enabled:
		scroll.mouse_filter = Control.MOUSE_FILTER_PASS
	else:
		scroll.mouse_filter = Control.MOUSE_FILTER_IGNORE

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
		cur_tween.kill()
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
	hover.emit(self)
	if not hover_enabled:
		return

	move_up()
	_hover = true

func _on_panel_container_mouse_exited() -> void:
	unhover.emit(self)
	
	move_down()
	_hover = false

func _on_card_panel_gui_input(event: InputEvent) -> void:
	input.emit(self, event)
