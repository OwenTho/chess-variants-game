extends Control

enum FILL_DIRECTION {
	LEFT,
	RIGHT
}

@export var progress_bar: ColorRect

@export var progress_speed: float = 5

@export var fill_direction: FILL_DIRECTION

var next_progress: Array[int] = []

var max_progress: float = 0
var progress: float = 0

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
	_update_visual()

func set_player(player_num: int) -> void:
	# Get the player data
	var player_id: int = Lobby.get_player_id_from_num(player_num)
	var info: Lobby.PlayerInfo = Lobby.get_player_info(player_id)
	
	$PlayerLabel.text = "Player %s" % [player_num + 1]
	if not Lobby.is_local and info != null:
		_set_name(info.name)
	else:
		_set_name("")

func _set_name(name: String) -> void:
	$NameLabel.text = name

func _update_visual() -> void:
	if progress_bar == null:
		return
	progress_bar.material.set_shader_parameter("fill_right", fill_direction == FILL_DIRECTION.RIGHT)
	progress_bar.material.set_shader_parameter("progress", clampf(progress / max_progress, 0.0, 1.0))

func _next_goal() -> void:
	next_progress.remove_at(0)

func add_next_progress(next_prog: int) -> void:
	next_progress.append(next_prog)
