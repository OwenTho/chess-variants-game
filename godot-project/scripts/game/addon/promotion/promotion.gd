extends GameAddon

func _init() -> void:
	add_card_notice("promotion", _on_promotion)

var cur_card: Node
var cur_promoting_piece: Node
var cur_selection: Array

func _on_promotion(card: Node) -> void:
	# Only server can promote
	if not is_multiplayer_authority():
		return
	
	# Get the promotion options
	cur_card = card
	cur_selection = card.toPiece
	
	cur_promoting_piece = card.promotingPiece
	
	# Send the options to the current player
	var player_id = Lobby.get_player_id_from_num(GameManager.game_controller.UnsafeGetCurrentPlayer())
	_give_promotion_options.rpc_id(player_id, cur_selection)

@rpc("authority", "call_local", "reliable")
func _give_promotion_options(options: Array) -> void:
	if !GameManager.in_game:
		return
	print(options)
	_pick_option.rpc(randi_range(0, options.size()-1))

@rpc("any_peer", "call_local", "reliable")
func _pick_option(option_ind: int) -> void:
	if !GameManager.in_game:
		return
	if !is_multiplayer_authority():
		return
	
	if cur_selection == null:
		return
	if option_ind < 0 or option_ind >= cur_selection.size():
		return
	
	var chosen_promotion = cur_selection[option_ind]
	_promote_piece.rpc(cur_promoting_piece.id, chosen_promotion)
	# Finally, send the card notice
	GameManager.send_card_notice(cur_card, "promoted")

@rpc("authority", "call_local", "reliable")
func _promote_piece(piece_id: int, new_piece: String) -> void:
	if !GameManager.in_game:
		return
	
	var info = GameManager.game_controller.GetPieceInfo(new_piece)
	if info == null:
		return
	
	var piece = GameManager.game_controller.UnsafeGetPiece(piece_id)
	if piece == null:
		return
	
	piece.info = info
	
