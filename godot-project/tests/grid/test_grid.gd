extends GutTest

var grid_script: CSharpScript
var grid_item: CSharpScript
var grid
var items: Array
var cells: Array

func before_all() -> void:
	grid_script = load("res://tests/grid/TestGrid.cs")
	grid_item = load("res://tests/grid/TestGridItem.cs")
	items = []
	cells = []


func before_each() -> void:
	if grid_script == null:
		print("huh")
	# Make a simple board
	grid = grid_script.new()
	
	# Put 5 items diagonally across the grid, with negatives to check
	# that negatives are possible.
	for i in range(5):
		var new_item = grid_item.new()
		items.append(new_item)
		cells.append(grid.PlaceItemAt(new_item, i-2, i-2))


func after_each() -> void:
	items = []
	cells = []




# If setup is successful, then adding items is also successful
func test_setup():
	assert_eq(grid.CellCount(), 5, "There should be exactly 5 cells.")
	
	# Make sure the 5 items are in the right place on the grid
	for i in range(5):
		assert_true(grid.HasCellAt(i-2,i-2), "Missing a cell on the diagonal.")
		assert_true(items[i].grid == grid, "Item %s should be on the grid." % [i])
		assert_true(items[i].cell != null, "Item %s should have a cell." % [i])
		assert_true(cells[i].grid == grid, "Cell %s should be on the grid." % [i])




func test_remove():
	for i in range(5):
		var pos = i - 2
		# Ensure this item is deleted correctly.
		assert_true(grid.RemoveItem(items[i]), "Item %s should be removed successfully." % [i])
		assert_false(grid.HasCellAt(pos, pos), "The cell of the item %s should have been removed." % [i])
		assert_eq(grid.CellCount(), 4 - i, "There should be %s cells after removing item %s." % [4 - i, i])
		assert_false(grid.HasItem(items[i]), "Grid should not still have item %s." % [i])
		assert_true(items[i].grid == null, "Item %s should not have a grid." % [i])
		assert_true(items[i].cell == null, "Item %s should not have a cell." % [i])
		
		# Ensure the rest of the items and cells are fine
		for j in range(5, i+1):
			assert_true(grid.HasItem(items[j]), "Grid should still have item %s." % [j])
			assert_true(grid.HasCellAt(j-2, j-2), "Grid should still have the cell at %s,%s" % [j-2, j-2])




func test_place_new():
	# Move 0,0 to 1,0
	var middle_item = items[2]
	var cell_before = cells[2]
	grid.PlaceItemAt(middle_item, 1, 0)
	
	assert_false(grid.HasCellAt(0,0), "The cell as 0,0 should no longer be there.")
	assert_true(grid.HasCellAt(1,0), "There should now be a cell at 1,0.")
	assert_true(cell_before == grid.GetCellAt(1,0), "The cell the item is in should have moved, not made a new one.")
	assert_true(middle_item.cell == cell_before, "The cell the item is in should have moved, not made a new one.")




func test_place_move():
	# Move all items to 0,0
	var middle_cell = grid.GetCellAt(0,0)
	var items_in_middle = 1
	
	for i in range(5):
		# Move the item to 0,0
		grid.PlaceItemAt(items[i], 0, 0)
		
		if i == 2:
			# Special case for middle item, as it shouldn't move.
			assert_true(items[i].cell == middle_cell, "Item %s should have stayed in the middle cell." % [i])
			assert_true(grid.HasCellAt(0,0), "There should still be a cell at 0,0.")
			assert_true(items[i].cell.ItemCount() == items_in_middle, "There should still be %s items in the middle cell." % [items_in_middle])
		else:
			# Make sure the item was moved, and the cell removed
			items_in_middle += 1
			assert_true(items[i].cell == middle_cell, "Item %s should have been moved to the middle cell." % [i])
			assert_true(items[i].cell.ItemCount() == items_in_middle, "There should be %s items in the middle cell." % [items_in_middle])
			assert_false(grid.HasCellAt(i-2,i-2), "Cell should have been removed at %s,%s after item %s was moved." % [i-2, i-2, i])




func test_swap():
	# Test directly swapping (-1,-1 and 0,0 - Swapping with 0,0)
	var cell2 = cells[1]
	var cell3 = cells[2]
	var x2 = cell2.x
	var y2 = cell2.y
	var x3 = cell3.x
	var y3 = cell3.y
	grid.SwapCells(cell2, cell3)
	
	assert_eq(cell2.x, x3)
	assert_eq(cell2.y, y3)
	assert_true(cell2.grid == grid, "Cell 2 should still be on the grid.")
	assert_eq(cell3.x, x2)
	assert_eq(cell3.y, y2)
	assert_true(cell3.grid == grid, "Cell 3 should still be on the grid.")
	
	# Test swapping by position (-2,-2, 1,1 - Swapping positive with negative)
	var cell1 = cells[0]
	var cell4 = cells[3]
	var x1 = cell1.x
	var y1 = cell1.y
	var x4 = cell4.x
	var y4 = cell4.y
	grid.SwapCellsAt(x1, y1, x4, y4)
	
	assert_eq(cell1.x, x4)
	assert_eq(cell1.y, y4)
	assert_true(cell1.grid == grid, "Cell 1 should still be on the grid.")
	assert_eq(cell4.x, x1)
	assert_eq(cell4.y, y1)
	assert_true(cell4.grid == grid, "Cell 4 should still be on the grid.")
	
	# Test swapping with null (2,2)
	grid.SwapCells(cells[4], null)
	
	assert_true(cells[4].grid == null, "Cell should no longer be on the grid.")
