shader_type canvas_item;

uniform vec4 highlight_color : source_color;

void fragment() {
	COLOR = vec4(highlight_color.rgb, min(highlight_color.a, COLOR.a));
}
