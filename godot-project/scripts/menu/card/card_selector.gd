class_name CardSelector
extends Node

class SelectionInfo:
	var player_num: int = -1
	var team_id: int = -1
	
	func _init(player_num: int, team_id: int = -1) -> void:
		self.player_num = player_num
		self.team_id = team_id
	
	func _reset_next() -> void:
		push_error("_reset_next not defined in SelectionInfo extended class.")
	
	func _get_next() -> CardBase:
		push_error("_get_next not defined in SelectionInfo extended class.")
		return null
	
	func _return_card(card: CardBase) -> void:
		card.queue_free()
		push_error("_return_card not defined in SelectionInfo extended class.")
	
# Card Selection
class DeckSelectionInfo extends SelectionInfo:
	var deck: Node
	var _cur_taken: int = 0
	var quantity: int
	
	func _init(player_num: int, deck, quantity: int, team_id: int = -1) -> void:
		super._init(player_num, team_id)
		self.deck = deck
		self.quantity = quantity
	
	func _reset_next() -> void:
		_cur_taken = 0
	
	func _get_next() -> CardBase:
		if _cur_taken >= quantity:
			return null
		_cur_taken += 1
		return GameManager.game_controller.PullCardFromDeck(deck)
	
	func _return_card(card: CardBase) -> void:
		# Return to deck and free card
		card.queue_free()
		GameManager.game_controller.ReturnCardToDeck(card, deck)

class CustomSelectionCardGetter:
	
	func _get_card() -> CardBase:
		return null
	
	func _return_card(card) -> bool:
		card.queue_free()
		return false



class CustomSelectionDeck extends CustomSelectionCardGetter:
	var deck
	var remove_card: bool = false
	func _init(deck, remove_card: bool) -> void:
		self.deck = deck
		self.remove_card = remove_card
	
	func _get_card() -> CardBase:
		var card = GameManager.game_controller.PullCardFromDeck(deck)
		# If not removing card, re-add it to the deck
		if card != null and not remove_card:
			GameManager.game_controller.ReturnCardToDeck(card, deck)
		return card
	
	func _return_card(card: CardBase) -> bool:
		# If remove_card is enabled, it has already
		# been returned
		card.queue_free()
		if not remove_card:
			return true
		return GameManager.game_controller.ReturnCardToDeck(card, deck)

class CustomSelectionFactory extends CustomSelectionCardGetter:
	var factory
	func _init(factory) -> void:
		self.factory = factory
	
	func _get_card() -> CardBase:
		return GameManager.game_controller.MakeCardUsingFactory(factory)
	
	func _return_card(card: CardBase) -> bool:
		card.queue_free()
		return factory.ReturnCard(card)

class CustomSelectionInfo extends SelectionInfo:
	var card_getters: Array[CustomSelectionCardGetter]
	var _cur: int = 0
	var _cur_cards: Array = []
	
	func add_card_getter(card_getter: CustomSelectionCardGetter) -> void:
		card_getters.append(card_getter)
	
	func _reset_next() -> void:
		_cur = 0
		_cur_cards.clear()
	
	func _get_next() -> CardBase:
		if _cur < 0 or _cur >= card_getters.size():
			return null
		
		var card: CardBase = null
		while card == null:
			card = card_getters[_cur]._get_card()
			_cur += 1
		if card != null:
			_cur_cards.append(card)
		return card
	
	func _return_card(card: CardBase) -> void:
		# Free card
		card.queue_free()
		for i in range(0,_cur_cards.size()):
			var getter = card_getters[i]
			if card == _cur_cards[i]:
				getter._return_card(card)
				break



# For converting the Card into a dictionary
var game_controller

var _cur: int = -1
var currently_selecting: int :
	get: return _cur

var _selections: Array[SelectionInfo] = []
var _selecting: bool = false

# Right before starting a new selection
signal before_new_selection()

# An option for selection was added
signal card_option_added(card_data: Dictionary)

# Starting a new selection
signal selection_started()

# An invalid card was selected
signal invalid_selection(card_num: int)

# Card was selected
signal card_selected(card)

# Selection is done
signal selection_done()

# Called once all current selections are done
signal all_selections_done()

# Private signal called when select_card is called
signal _select_card(card_num: int)

func add_card_selection(player_num: int, deck, quantity: int, team_id: int = -1) -> void:
	if deck == null:
		push_error("Can't select from an invalid deck.")
		return
	# Add to the selections to make
	_selections.append(DeckSelectionInfo.new(player_num, deck, quantity, team_id))

func create_custom_selection(player_num: int, team_id: int = -1) -> CustomSelectionInfo:
	return CustomSelectionInfo.new(player_num, team_id)

func add_custom_selection(custom_selection: CustomSelectionInfo) -> void:
	if custom_selection == null:
		push_error("Can't select from a null selection.")
		return
	_selections.append(custom_selection)

func select() -> void:
	if _selecting:
		push_error("Can't start a new selection when already selecting.")
		return
	_next_select()

func _next_select() -> void:
	_selecting = true
	var selections = _selections
	_selections = []
	var selected_cards: Array = []
	while not selections.is_empty():
		# Get first and remove it from the list
		var cur_selection: SelectionInfo = selections[0]
		selections.remove_at(0)
		# Set the currently selecting player for the GameManager
		_cur = cur_selection.player_num
		if cur_selection.player_num < 0:
			push_error("Tried to have an invalid player select a card.")
			continue
		before_new_selection.emit()
		var player_cards: Array[CardBase] = []
		cur_selection._reset_next()
		var cur_card_num: int = 0
		# Pull the first
		var new_card: CardBase = await cur_selection._get_next()
		# If new card is null, break as it has reached the end of the card list
		while new_card != null:
			# Temporarily add as a child to avoid possible memory leak
			add_child(new_card)
			# Set its team
			new_card.teamId = cur_selection.team_id
			player_cards.append(new_card)
			# Send the card to the player
			var card_data: Dictionary = game_controller.ConvertCardToDict(new_card)
			# Add the card number
			card_data.card_num = cur_card_num
			card_option_added.emit(card_data)
			cur_card_num += 1
			# Pull a new card
			new_card = await cur_selection._get_next()
		
		# If there are no cards, break out of the loop
		if player_cards.size() == 0:
			break
		
		# Tell the player to display the cards
		selection_started.emit()
		# Wait for the player to select the card
		var selected_card: int = -1
		while selected_card < 0 or selected_card >= player_cards.size():
			selected_card = await _select_card
			if selected_card < 0 or selected_card >= player_cards.size():
				invalid_selection.emit(selected_card)
		
		# Free the unused cards
		for j in range(player_cards.size()):
			if j != selected_card:
				cur_selection._return_card(player_cards[j])
		# Remove the child now that we can add it to the game
		remove_child(player_cards[selected_card])
		var card_data = game_controller.ConvertCardToDict(player_cards[selected_card])
		if card_data == null:
			continue
		# Emit the signal that the card was selected
		card_selected.emit(player_cards[selected_card])
		selection_done.emit()
		_cur = -1
	_selecting = false
	# Now that cards have been added, emit the signal
	all_selections_done.emit()

func select_card(card_num: int) -> void:
	_select_card.emit(card_num)
