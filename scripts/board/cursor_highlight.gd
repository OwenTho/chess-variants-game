extends BoardItem2D

var input_mouse: bool = true
var can_move: bool = false

func _ready():
	Debug.stats.add_property(self, "x")
	Debug.stats.add_property(self, "y")

func _process(event):
	# For mouse, do every frame (as that's what the player will see)
	if input_mouse:
		# Find the cell the mouse is hovering
		check_mouse()
		
		return
	
func check_mouse() -> void:
	# Find the cell the mouse is hovering
	var mouse_pos: Vector2 = get_global_mouse_position()
	var mouse_cell: Vector2i = board.global_world_to_board_coord(mouse_pos)
	
	set_pos(mouse_cell.x, mouse_cell.y)
	
	return
