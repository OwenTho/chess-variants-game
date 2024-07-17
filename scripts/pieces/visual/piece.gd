extends BoardItem2D

class_name Piece2D

var piece_data

func _ready() -> void:
	piece_data.ChangedCell.connect(_on_move)

func _on_move(new_cell) -> void:
	if new_cell != null:
		set_pos(new_cell.x, new_cell.y)
	else:
		queue_free()

func update_pos() -> void:
	set_pos(piece_data.cell.x, piece_data.cell.y)

func set_sprite(new_sprite: Texture2D):
	$SprPiece.texture = new_sprite
	
	# Scale based on board
	# If there is no board, use default scale
	if board == null:
		$SprPiece.scale = Vector2(1,1)
		return
	
	var tex_scale_x: float = (board.board_width as float) / new_sprite.get_width()
	var tex_scale_y: float = (board.board_height as float) / new_sprite.get_height()
	
	$SprPiece.scale = Vector2(tex_scale_x, tex_scale_y)

func update_sprite() -> void:
	var image_loc: String = "assets/texture/piece/" + piece_data.info.textureLoc
	var piece_sprite: Texture
	if FileAccess.file_exists("res://" + image_loc):
		piece_sprite = load("res://" + image_loc)
	else:
		push_warning("Could not find sprite at path '%s', so defalt is being used." % [image_loc])
		piece_sprite = load("res://assets/texture/piece/default.png")
	
	set_sprite(piece_sprite)
