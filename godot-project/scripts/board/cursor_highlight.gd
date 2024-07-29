extends BoardItem2D

var input_mouse: bool = true
var can_move: bool = false

var last_cell: Vector2i = Vector2.ZERO

signal cell_updated(new_cell: Vector2i)

var active: bool = true

func _update_cell(new_cell: Vector2i):
	if new_cell != last_cell:
		last_cell = new_cell
		cell_updated.emit(new_cell)
		set_pos(new_cell.x, new_cell.y)

func _process(_delta):
	if not active:
		visible = false
		return
	else:
		visible = true
	# For mouse, do every frame (as that's what the player will see)
	if input_mouse:
		# Find the cell the mouse is hovering
		check_mouse()
		return
	
	# For Controller, check on each input + with a small
	# delay to allow more control

func check_mouse() -> void:
	# Find the cell the mouse is hovering
	var mouse_pos: Vector2 = get_global_mouse_position()
	var mouse_cell: Vector2i = board.global_world_to_board_coord(mouse_pos)
	
	_update_cell(mouse_cell)
