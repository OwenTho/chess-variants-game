extends Control

@export var hover_time: float = 0.3
@export var hover_offset: float = -20
@export_range(1,30,1,"or_greater") var default_font_size: int = 24

var cur_tween: Tween

var card_id: int = -1

var hold_up: bool = false
var _hover: bool = false
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

func _on_panel_container_mouse_entered() -> void:
	hover.emit(card_id)
	
	# Move card to be visually higher
	if cur_tween != null:
		cur_tween.stop()
	if not _hover:
		cur_tween = create_tween()
		cur_tween.tween_property(card_panel, "position:y", hover_offset, hover_time)
		cur_tween.tween_callback(_tween_end)
		_hover = true


func _on_panel_container_mouse_exited() -> void:
	unhover.emit(card_id)
	
	# Move card back down
	if cur_tween != null:
		cur_tween.stop()
	if _hover and not hold_up:
		cur_tween = create_tween()
		cur_tween.tween_property(card_panel, "position:y", 0.0, hover_time)
		cur_tween.tween_callback(_tween_end)
		_hover = false
