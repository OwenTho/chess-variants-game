extends Node2D

@export var board: Board2D
@export var cursor: BoardItem2D

@export var action_highlights: Node2D
@export var highlight_scene: PackedScene

var selected_piece: Piece2D
var possible_actions: Array

func _ready() -> void:
	GameManager.has_init.connect(_on_init)

func _on_init():
	cursor.board = GameManager.board

func _process(delta) -> void:
	if Input.is_action_pressed("mouse_right"):
		rotation = rotation + deg_to_rad(45) * delta

func _input(event) -> void:
	if event is InputEventMouseButton and event.is_action_pressed("mouse_left"):
		
		# Get the piece the player is selecting
		var cell_pos: Vector2i = cursor.last_cell
		
		select_cell(cell_pos)

func remove_selection() -> void:
	selected_piece = null
	possible_actions = []
	for child in action_highlights.get_children():
		child.queue_free()

func select_cell(cell_pos: Vector2i):
	# First check if the player is selecting an action
	var actions_to_take: Array = []
	for action in possible_actions:
		if action.actionLocation == cell_pos:
			actions_to_take.append(action)
	
	if actions_to_take.size() > 0:
		for action in actions_to_take:
			action.ActOn(selected_piece.piece_data)
		remove_selection()
		return
	
	# If not any of the above, check if there is a cell on the Grid
	var cell = GameManager.grid.GetCellAt(cell_pos.x, cell_pos.y)
	
	# Remove existing selection before moving on	
	remove_selection()
	
	# If cell doesn't exist, ignore
	if cell == null:
		return
	
	# Get the first piece
	var item = cell.GetItem(0)
	var item_node = item.get_parent()
	
	select_item(item_node)

func select_item(piece: Piece2D) -> void:
	selected_piece = piece
	possible_actions = piece.piece_data.GetPossibleActions(GameManager.game_controller)
	
	for action in possible_actions:
		var new_highlight: Node2D = highlight_scene.instantiate()
		
		new_highlight.board = board
		new_highlight.set_pos(action.actionLocation.x, action.actionLocation.y)
		action_highlights.add_child.call_deferred(new_highlight)
