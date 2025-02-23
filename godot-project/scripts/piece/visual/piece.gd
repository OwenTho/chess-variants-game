extends BoardItem2D

class_name Piece2D

@export var sprite: Sprite2D
@export var sprite_transform_node: Node2D

signal actions_updated(piece: Piece2D)

var piece_data: Piece
var id: int:
	get:
		if piece_data == null:
			return -1
		return piece_data.Id

var _cur_move_tween: Tween
var _cur_info_tween: Tween

var possible_actions: Array[Vector2i] = []

var taken = false


func _ready() -> void:
	piece_data.ChangedCell.connect(_on_move)
	piece_data.InfoChanged.connect(_on_info_changed)
	
	$PieceAnimator.animation_finished.connect(_on_animation_finished)

func _process(delta: float) -> void:
	sprite_transform_node.z_index = clampi(global_position.y, RenderingServer.CANVAS_ITEM_Z_MIN, RenderingServer.CANVAS_ITEM_Z_MAX)

func set_actions(action_locations: Array[Vector2i]) -> void:
	if action_locations == null:
		possible_actions = []
		return
	possible_actions = action_locations
	actions_updated.emit(self)

func take_piece() -> void:
	# Only take piece if not already taken
	if taken:
		return
	taken = true
	# Do Take Piece visuals / audio
	visible = false
	$PieceAnimator.play("take_animation")

func _on_animation_finished(anim_name: String) -> void:
	if anim_name == "take_animation":
		queue_free()

func reset_actions() -> void:
	set_actions([])

func _on_move(new_cell) -> void:
	if new_cell != null and new_cell.Pos != pos:
		if _cur_move_tween != null:
			_cur_move_tween.kill()
		_cur_move_tween = create_tween()
		var move_pos: Vector2 = board.board_to_world_coord(new_cell.Pos)
		_cur_move_tween.tween_property(self, "position", move_pos, 0.2)
		_cur_move_tween.tween_callback(_move_tween_end)
		pos = new_cell.Pos
		# Only play sound after the start of the game - This avoids Shuffle
		# playing the sound for all pieces.
		if GameManager.turn_number > 0:
			$MoveSound.play()

func update_pos() -> void:
	if piece_data == null:
		return
	if piece_data.Cell == null:
		return
	set_pos(piece_data.Cell.X, piece_data.Cell.Y)

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

func update_sprite() -> void:
	set_sprite(GameResources.get_piece_texture_from_piece(piece_data))
	
	if piece_data == null:
		sprite.material = null
		return
	
	sprite.material = GameManager.get_new_team_material(piece_data.TeamId)

func set_selected(selected: bool) -> void:
	sprite.material.set_shader_parameter("outline_enabled", selected)

func _on_info_changed(_info) -> void:
	if _cur_info_tween != null:
		_cur_info_tween.kill()
	# Reset rotation of sprite
	sprite.rotation = 0
	_cur_info_tween = create_tween()
	_cur_info_tween.tween_property(sprite, "rotation", deg_to_rad(180), 0.1)
	# Get sprite now, as data may be freed when the tween gets to it
	_cur_info_tween.tween_callback(set_sprite.bind(GameResources.get_piece_texture_from_piece(piece_data)))
	_cur_info_tween.tween_property(sprite, "rotation", deg_to_rad(360), 0.1)
	_cur_info_tween.tween_callback(_info_tween_end)

func _move_tween_end() -> void:
	_cur_move_tween = null

func _info_tween_end() -> void:
	sprite.rotation = 0
	_cur_info_tween = null
