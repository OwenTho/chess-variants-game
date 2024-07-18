extends Node2D

# A class for assisting visualisation of a Grid
class_name Board2D

@export var board_width: int = 128
@export var board_height: int = 128

# Offset from board -> world coords Must be [0,board_width) and [0,board_height)
@export var coord_offset_x: int = 64
@export var coord_offset_y: int = -64

func board_to_world_coord(board_cell: Vector2i) -> Vector2:
	return Vector2(board_cell.x * board_width + coord_offset_x, -1 * board_cell.y * board_height + coord_offset_y)

func global_board_to_world_coord(board_cell: Vector2i) -> Vector2:
	return global_position + board_to_world_coord(board_cell).rotated(rotation) * global_scale

func _world_to_board_coord(world_coord: Vector2, pivot_point: Vector2, offset: Vector2):
	# Rotate the world coordinate to effectively make the board horizontal
	# for the calculation.
	var rotated_coord: Vector2 = world_coord - pivot_point
	rotated_coord = rotated_coord.rotated(-global_rotation)
	rotated_coord += pivot_point
	
	return Vector2i(floor((rotated_coord.x - offset.x) / board_width / global_scale.x), floor(-1 * (rotated_coord.y - offset.y) / board_height / global_scale.y))


func world_to_board_coord(world_coord: Vector2) -> Vector2i:
	return _world_to_board_coord(world_coord, Vector2.ZERO, Vector2.ZERO)

func global_world_to_board_coord(world_coord: Vector2) -> Vector2i:
	return _world_to_board_coord(world_coord, global_position, global_position)
