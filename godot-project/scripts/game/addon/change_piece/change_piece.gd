extends GameAddon

const START_NOTICE = "change_piece"
const FIN_NOTICE = "piece_changed"

var change_piece_info_id
var cur_card: Node

func _init() -> void:
	add_card_notice("minor_change_piece", START_NOTICE, _on_change_piece)

func _on_change_piece(card: Node) -> void:
	if not GameManager.in_game:
		send_card_notice(card, FIN_NOTICE)
		return
	if not is_multiplayer_authority():
		send_card_notice(card, FIN_NOTICE)
		return
	
	change_piece_info_id = card.toPiece
	cur_card = card
	
	_start_change_piece.rpc_id(Lobby.get_player_id_from_num(GameManager.unsafe_get_current_player()), change_piece_info_id)

@rpc("authority", "call_local", "reliable")
func _start_change_piece(to_piece_id: String) -> void:
	if not GameManager.in_game:
		return
	GameManager.receive_notice("Select a piece to change into a %s" % [to_piece_id])
	
	# Get the game screen's cursor
	var cursor = GameManager.game.cursor
	# Show it
	GameManager.game.show_cursor()
	# Add a signal for selection
	cursor.cell_selected.connect(_on_cursor_select)

func _on_cursor_select(pos: Vector2i) -> void:
	# Get the piece at that space
	var piece: Node = GameManager.unsafe_get_first_piece_at(pos.x, pos.y)
	if piece == null:
		return
	_select_change_piece.rpc(piece.id)

@rpc("any_peer", "call_local", "reliable")
func _select_change_piece(piece_id: int) -> void:
	if not GameManager.in_game:
		return
	if not is_multiplayer_authority():
		return
	
	if change_piece_info_id == null or cur_card == null:
		return
	
	# Try to get the piece
	var piece: Node = GameManager.unsafe_get_piece(piece_id)
	if piece == null:
		return
	
	# Tell everyone to change the piece
	_change_piece.rpc(piece.linkId, change_piece_info_id)
	
	# Finally send the card notice that it's done changing the piece
	send_card_notice(cur_card, FIN_NOTICE)
	
	cur_card = null
	change_piece_info_id = null
	

@rpc("authority", "call_local", "reliable")
func _change_piece(link_id: int, to_piece: String) -> void:
	if not GameManager.in_game:
		return
	
	# Hide the cursor
	GameManager.game.hide_cursor()
	
	# Disconnect the cursor signal if it's connected
	var cursor = GameManager.game.cursor
	if cursor.cell_selected.is_connected(_on_cursor_select):
		cursor.cell_selected.disconnect(_on_cursor_select)
	
	var info = GameManager.get_piece_info(to_piece)
	if info == null:
		push_warning("'%s' has no info, so piece info will be set to null." % [to_piece])
	
	# Get all pieces by their link id
	for piece in GameManager.unsafe_get_pieces_by_link_id(link_id):
		piece.info = info
		piece.EnableActionsUpdate()
