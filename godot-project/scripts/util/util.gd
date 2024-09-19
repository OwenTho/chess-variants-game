class_name Util
extends Node

static func debug_print(text: String) -> void:
	if OS.is_debug_build():
		print(text)
