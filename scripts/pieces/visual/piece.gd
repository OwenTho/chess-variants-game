extends BoardItem2D

class_name Piece2D

@onready var piece_data = $PieceData

func _ready() -> void:
	piece_data.ChangedCell.connect(_on_move)

func _on_move(new_cell) -> void:
	set_pos(new_cell.x, new_cell.y)

func set_sprite(new_sprite: Texture2D):
	$SprPiece.texture = new_sprite
	
	# Scale based on board
	if board == null:
		return
	
	var tex_scale_x: float = (board.board_width as float) / new_sprite.get_width()
	var tex_scale_y: float = (board.board_height as float) / new_sprite.get_height()
	
	$SprPiece.scale = Vector2(tex_scale_x, tex_scale_y)
