extends Control

enum FILL_DIRECTION {
	LEFT,
	RIGHT
}

@export var progress_bar: ColorRect
@export var turn_arrow: TextureRect

@export var progress_speed: float = 5

@export var fill_direction: FILL_DIRECTION

var next_progress: Array[int] = []

var _player_num: int = 0

var max_progress: float = 0
var progress: float = 0

var left_arrow: Texture = preload("res://assets/texture/ui/player_arrow_left.png")
var right_arrow: Texture = preload("res://assets/texture/ui/player_arrow_right.png")

func _ready() -> void:
	update_progress_visual(0, 1)
	update_visual_positioning()
	update_arrow_visual(-1)

func _process(delta: float) -> void:
	if next_progress.is_empty():
		return
	
	var goal = next_progress[0]
	
	if goal < progress:
		progress -= progress_speed * delta
		if progress <= goal:
			progress = goal
			_next_goal()
	else:
		progress += progress_speed * delta
		if progress >= goal:
			progress = goal
			_next_goal()
	update_progress_visual()

func set_player(player_num: int) -> void:
	# Get the player data
	var player_id: int = Lobby.get_player_id_from_num(player_num)
	var info: Lobby.PlayerInfo = Lobby.get_player_info(player_id)
	_player_num = player_num
	
	turn_arrow.material = GameManager.get_team_material(player_num)
	
	$PlayerLabel.text = str(player_num + 1)
	if not Lobby.is_local and info != null:
		_set_name(info.name)
	else:
		_set_name("")

func _set_name(name: String) -> void:
	$NameLabel.text = name

func update_arrow_visual(cur_player: int) -> void:
	turn_arrow.visible = cur_player == _player_num

func update_progress_visual(display_progress: float = progress, target_progress: float = max_progress) -> void:
	if progress_bar == null:
		return
	progress_bar.material.set_shader_parameter("fill_right", fill_direction == FILL_DIRECTION.RIGHT)
	progress_bar.material.set_shader_parameter("progress", clampf(display_progress / target_progress, 0.0, 1.0))

func update_visual_positioning() -> void:
	var horizontal_alignment = HORIZONTAL_ALIGNMENT_LEFT
	match fill_direction:
		FILL_DIRECTION.RIGHT:
			horizontal_alignment = HORIZONTAL_ALIGNMENT_LEFT
			turn_arrow.texture = left_arrow
			$ArrowPositioner.alignment = HBoxContainer.AlignmentMode.ALIGNMENT_END
		FILL_DIRECTION.LEFT:
			horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
			turn_arrow.texture = right_arrow
			$ArrowPositioner.alignment = HBoxContainer.AlignmentMode.ALIGNMENT_BEGIN
	$PlayerLabel.horizontal_alignment = horizontal_alignment
	$NameLabel.horizontal_alignment = horizontal_alignment

func _next_goal() -> void:
	next_progress.remove_at(0)

func add_next_progress(next_prog: int) -> void:
	next_progress.append(next_prog)
