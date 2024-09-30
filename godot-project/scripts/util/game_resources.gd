class_name GameResources
extends Node

static func get_piece_texture_from_info(piece_info) -> Texture2D:
	# If null, use the error sprite
	var image_loc: String = "assets/texture/piece/invalid_piece.png"
	if piece_info != null:
		image_loc = "assets/texture/piece/" + piece_info.textureLoc
	
	var piece_sprite: Texture
	if ResourceLoader.exists("res://" + image_loc):
		piece_sprite = load("res://" + image_loc)
	else:
		push_warning("Could not find sprite at path '%s', so defalt is being used." % [image_loc])
		piece_sprite = load("res://assets/texture/piece/default.png")
	return piece_sprite

static func get_piece_texture_from_piece(piece_data: Piece) -> Texture2D:
	if piece_data == null or piece_data.info == null:
		return get_piece_texture_from_info(null)
	return get_piece_texture_from_info(piece_data.info)

static func get_piece_texture(piece_id: String) -> void:
	# TRY to get the piece data
	var piece_info = GameManager.game_controller.GetPieceInfo(piece_id)
	
	return get_piece_texture_from_info(piece_info)


static func get_card_texture_from_loc(loc: String) -> Texture2D:
	# If null, use the error sprite
	var image_loc: String = "assets/texture/card/" + loc
	
	var card_sprite: Texture
	if ResourceLoader.exists("res://" + image_loc):
		card_sprite = load("res://" + image_loc)
	else:
		push_warning("Could not find sprite at path '%s', so default is being used." % [image_loc])
		card_sprite = load("res://assets/texture/card/missing.png")
	return card_sprite

static func get_card_texture(card_data: CardBase) -> Texture2D:
	if card_data == null:
		return get_card_texture_from_loc("missing.png")
	return get_card_texture_from_loc(card_data.GetCardImageLoc())
