extends BoardItem2D

class_name Piece2D

var piece_data

func _ready() -> void:
	piece_data.ChangedCell.connect(_on_move)
	piece_data.InfoChanged.connect(info_changed)

func _on_move(new_cell) -> void:
	if new_cell != null:
		set_pos(new_cell.x, new_cell.y)

func update_pos() -> void:
	if piece_data == null:
		return
	if piece_data.cell == null:
		return
	set_pos(piece_data.cell.x, piece_data.cell.y)

func _enter_tree():
	update_scale()

func update_scale():
	var tex_scale_x: float = (board.board_width as float) / $SprPiece.texture.get_width() * global_scale.x
	var tex_scale_y: float = (board.board_height as float) / $SprPiece.texture.get_height() * global_scale.y
	
	$SprPiece.scale = Vector2(tex_scale_x, tex_scale_y)

func set_sprite(new_sprite: Texture2D):
	$SprPiece.texture = new_sprite
	
	# Scale based on board
	# If there is no board, use default scale
	if board == null:
		$SprPiece.scale = Vector2(1,1)
		return
	
	update_scale()

func update_sprite() -> void:
	# If null, use the error sprite
	var image_loc: String = "assets/texture/piece/invalid_piece.png"
	var team_id: int = 0
	if piece_data.info != null:
		image_loc = "assets/texture/piece/" + piece_data.info.textureLoc
		team_id = piece_data.teamId
	print("Using image %s" % [image_loc])
	var piece_sprite: Texture
	if ResourceLoader.exists("res://" + image_loc):
		piece_sprite = load("res://" + image_loc)
	else:
		push_warning("Could not find sprite at path '%s', so defalt is being used." % [image_loc])
		piece_sprite = load("res://assets/texture/piece/default.png")
	
	set_sprite(piece_sprite)
	
	if team_id == 0:
		$SprPiece.material.set_shader_parameter("highlight_color", Color.STEEL_BLUE)
	else:
		$SprPiece.material.set_shader_parameter("highlight_color", Color.MAROON)

func info_changed(_info) -> void:
	update_sprite()
