extends Node2D

class_name BoardItem2D

var board: Board2D
var pos: Vector2i

func set_pos(x: int, y: int) -> void:
	self.pos = Vector2i(x,y)
	position = board.board_to_world_coord(self.pos)

func set_global_pos(x: int, y: int) -> void:
	self.pos = Vector2i(x,y)
	global_position = board.global_board_to_world_coord(self.pos)
