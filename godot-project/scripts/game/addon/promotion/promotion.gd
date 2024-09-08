extends GameAddon

const CARD_ID = "promotion"
const START_NOTICE = "promotion"
const FIN_NOTICE = "promoted"

func _init() -> void:
	add_card_notice("minor_promotion", "promotion", _on_promotion)

var promotion_selector_scene: PackedScene = preload("res://scenes/game/addons/promotion/promotion_selector.tscn")

var cur_card: Node
var cur_promoting_piece: Node
var cur_selection: Array = []

var cur_selection_node: Node2D

func _on_promotion(card: Node) -> void:
	if not GameManager.in_game:
		send_card_notice(card, FIN_NOTICE)
		return
	# Only server can promote
	if not is_multiplayer_authority():
		push_error("Client should not process %s card." % [CARD_ID])
		send_card_notice(card, FIN_NOTICE)
		return
	
	# Get the promotion options
	cur_card = card
	cur_selection = card.toPiece.duplicate(true)
	
	cur_promoting_piece = card.promotingPiece
	
	# Send the options to the current player
	var player_id = Lobby.get_player_id_from_num(GameManager.game_controller.UnsafeGetCurrentPlayer())
	_give_promotion_options.rpc_id(player_id, cur_promoting_piece.id, cur_selection)

@rpc("authority", "call_local", "reliable")
func _give_promotion_options(piece_id: int, options: Array) -> void:
	if !GameManager.in_game:
		return
	# If there is an existing selection, free it
	if cur_selection_node != null and is_instance_valid(cur_selection_node):
		cur_selection_node.queue_free()
		cur_selection_node = null
	
	# Make a new selection
	cur_selection_node = promotion_selector_scene.instantiate()
	
	# Add all of the options
	for i in range(options.size()):
		var option_info = GameManager.game_controller.GetPieceInfo(options[i])
		cur_selection_node.add_option(option_info, i)
	
	# Add the signal
	cur_selection_node.selection_made.connect(_on_selection_made)
	
	# Get the piece by the id
	var piece: Piece2D = GameManager.get_piece_id(piece_id)
	# Add the selector as a child
	piece.sprite_transform_node.add_child(cur_selection_node)

func _on_selection_made(piece_info, option_id) -> void:
	_pick_option.rpc(option_id)

@rpc("any_peer", "call_local", "reliable")
func _pick_option(option_ind: int) -> void:
	if !GameManager.in_game:
		return
	if !is_multiplayer_authority():
		return
	
	if cur_selection.is_empty():
		return
	if option_ind < 0 or option_ind >= cur_selection.size():
		print("Invalid option index: %s, 0 <= x < %s" % [option_ind, cur_selection.size()])
		return
	
	# Get the variables to send
	var chosen_promotion = cur_selection[option_ind]
	var card = cur_card
	var piece = cur_promoting_piece
	
	# Reset the variables (so that players can't double select)
	cur_card = null
	cur_promoting_piece = null
	cur_selection.clear()
	
	# Send the players the promotion to make
	_promote_piece.rpc(piece.id, chosen_promotion)
	# Finally, send the card notice
	send_card_notice(card, FIN_NOTICE)

@rpc("authority", "call_local", "reliable")
func _promote_piece(piece_id: int, new_piece: String) -> void:
	if !GameManager.in_game:
		return
	
	# Free the promotion node
	if cur_selection_node != null:
		cur_selection_node.queue_free()
		cur_selection_node.selection_made.disconnect(_on_selection_made)
		cur_selection_node = null
	
	var info = GameManager.game_controller.GetPieceInfo(new_piece)
	if info == null:
		push_warning("'%s' has no info, so piece info will be set to null." % [new_piece])
	
	var piece = GameManager.game_controller.UnsafeGetPiece(piece_id)
	if piece == null:
		push_error("Couldn't promote piece as there is no piece with id %s." % [piece_id])
		return
	
	piece.info = info
	
