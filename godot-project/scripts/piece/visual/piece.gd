extends BoardItem2D

class_name Piece2D

var piece_data
var _cur_move_tween: Tween
var _cur_info_tween: Tween

@export var sprite: Sprite2D
@export var sprite_transform_node: RemoteTransform2D

func _ready() -> void:
	piece_data.ChangedCell.connect(_on_move)
	piece_data.InfoChanged.connect(_on_info_changed)

func _process(delta: float) -> void:
	sprite.z_index = clampi(global_position.y, RenderingServer.CANVAS_ITEM_Z_MIN, RenderingServer.CANVAS_ITEM_Z_MAX)

func _on_move(new_cell) -> void:
	if new_cell != null:
		if _cur_move_tween != null:
			_cur_move_tween.kill()
		_cur_move_tween = create_tween()
		var move_pos: Vector2 = board.board_to_world_coord(piece_data.cell.pos)
		_cur_move_tween.tween_property(self, "position", move_pos, 0.2)
		_cur_move_tween.tween_callback(_move_tween_end)

func update_pos() -> void:
	if piece_data == null:
		return
	if piece_data.cell == null:
		return
	set_pos(piece_data.cell.x, piece_data.cell.y)

func _enter_tree():
	update_scale()

func update_scale():
	var tex_height: float = sprite.texture.get_height()

	sprite.position.y = -sprite.scale.y * tex_height / 3

func set_sprite(new_sprite: Texture2D):
	sprite.texture = new_sprite
	
	# Scale based on board
	# If there is no board, reset the sprite y position
	if board == null:
		sprite.position.y = 0
		return
		
	update_scale()

func get_sprite() -> Texture2D:
	# If null, use the error sprite
	var image_loc: String = "assets/texture/piece/invalid_piece.png"
	var team_id: int = 0
	if piece_data != null and piece_data.info != null:
		image_loc = "assets/texture/piece/" + piece_data.info.textureLoc
		team_id = piece_data.teamId
	# print("Using image %s" % [image_loc])
	var piece_sprite: Texture
	if ResourceLoader.exists("res://" + image_loc):
		piece_sprite = load("res://" + image_loc)
	else:
		push_warning("Could not find sprite at path '%s', so defalt is being used." % [image_loc])
		piece_sprite = load("res://assets/texture/piece/default.png")
	return piece_sprite

func update_sprite() -> void:
	set_sprite(get_sprite())
	
	if piece_data == null:
		return
	
	if piece_data.teamId == 0:
		sprite.material.set_shader_parameter("palette_colours", [
			Color.from_string("#656565", Color.WHITE),
			Color.from_string("#9d9d9d", Color.WHITE),
			Color.from_string("#ffffff", Color.WHITE)
		])
	else:
		sprite.material.set_shader_parameter("palette_colours", [
			Color.from_string("#1e1e1e", Color.WHITE),
			Color.from_string("#515151", Color.WHITE),
			Color.from_string("#747474", Color.WHITE)
		])

func _on_info_changed(_info) -> void:
	if _cur_info_tween != null:
		_cur_info_tween.kill()
	_cur_info_tween = create_tween()
	_cur_info_tween.tween_property(sprite, "rotation", deg_to_rad(180), 0.1)
	# Get sprite now, as data may be freed when the tween gets to it
	_cur_info_tween.tween_callback(set_sprite.bind(get_sprite()))
	_cur_info_tween.tween_property(sprite, "rotation", deg_to_rad(360), 0.1)
	_cur_info_tween.tween_callback(_info_tween_end)

func _move_tween_end() -> void:
	_cur_move_tween = null

func _info_tween_end() -> void:
	_cur_info_tween = null
