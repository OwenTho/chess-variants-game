extends BoardItem2D

var input_mouse: bool = true
var can_move: bool = false

var last_cell: Vector2i = Vector2.ZERO

signal cell_updated(new_cell: Vector2i)
signal cell_selected(pos: Vector2i)

var active: bool = true
var visible_on_active: bool = true

func _update_cell(new_cell: Vector2i):
	if new_cell != last_cell:
		last_cell = new_cell
		cell_updated.emit(new_cell)
		set_pos(new_cell.x, new_cell.y)

func check_mouse() -> void:
	# Find the cell the mouse is hovering
	var mouse_pos: Vector2 = get_global_mouse_position()
	var mouse_cell: Vector2i = board.global_world_to_board_coord(mouse_pos)
	
	_update_cell(mouse_cell)

func _process_input(event: InputEvent) -> void:
	if not active:
		return
	if event is InputEventMouseMotion:
		if input_mouse:
			check_mouse()
		return
	if event is InputEventMouseButton:
		if event.is_action_pressed("mouse_left"):
			cell_selected.emit(last_cell)
		return
