extends Node

class_name CardSelector

# Card Selection
class SelectionInfo:
	var player_num: int
	
	var deck
	var quantity: int
	
	func _init(player_num: int, deck, quantity: int) -> void:
		self.player_num = player_num
		self.deck = deck
		self.quantity = quantity

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

func add_card_selection(player_num: int, deck, quantity) -> void:
	if deck == null:
		push_error("Can't select from an invalid deck.")
		return
	# Add to the selections to make
	_selections.append(SelectionInfo.new(player_num, deck, quantity))

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
			push_warning("Tried to have an invalid player select a card.")
			continue
		before_new_selection.emit()
		var player_cards: Array[Node] = []
		for j in range(cur_selection.quantity):
			# Pull a new card
			# Await as it uses the game mutex
			var new_card = await game_controller.PullCardFromDeck(cur_selection.deck)
			# If new card is null, break as there is no more cards available
			if new_card == null:
				break
			# Temporarily add as a child to avoid possible memory leak
			add_child(new_card)
			player_cards.append(new_card)
			# Send the card to the player
			var card_data: Dictionary = game_controller.ConvertCardToDict(new_card)
			# Add the card number
			card_data.card_num = j
			card_option_added.emit(card_data)
		
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
				game_controller.ReturnCardToDeck(player_cards[j], cur_selection.deck)
				player_cards[j].queue_free()
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
