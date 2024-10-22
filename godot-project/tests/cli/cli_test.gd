extends Control

func _ready():
	var template_file = FileAccess.open("res://tests/scene_test_output.xml", FileAccess.READ)
	var content = template_file.get_as_text()
	
	var output_file = FileAccess.open("res://direct_scene_xml_output.xml", FileAccess.WRITE)
	
	template_file.close()
	output_file.close()
	
	output_file.store_string(content)
	get_tree().quit()
