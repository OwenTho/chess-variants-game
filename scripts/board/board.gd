extends Node2D

@export var board: Board2D
@export var cursor_highlight: BoardItem2D

func _ready() -> void:
	GameManager.start(board)
	cursor_highlight.board = board

func _process(delta) -> void:
	if Input.is_action_pressed("mouse_right"):
		rotation = rotation + deg_to_rad(45) * delta

func _input(event) -> void:
	if event is InputEventMouseButton and event.is_action_pressed("mouse_left"):
		for piece in get_tree().get_nodes_in_group("piece"):
			var cell = piece.piece_data.cell
			GameManager.grid.PlaceItemAt(piece.piece_data, cell.x, cell.y + 1)
