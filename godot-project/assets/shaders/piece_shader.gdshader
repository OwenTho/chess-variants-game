shader_type canvas_item;

// Palette settings
uniform int palette_count : hint_range(1, 10, 1);
uniform vec4 palette_colours[3] : source_color;
uniform sampler2D palette_colours_old;

void fragment() {
	// Implemented from https://godotshaders.com/shader/extensible-color-palette/
	float grey_value = max(COLOR.r, COLOR.g);
	grey_value = max(grey_value, COLOR.b);
	
	float multiplier = float(palette_colours.length()-1);
	float multiplied_grey = grey_value * multiplier;
	
	int rounded_grey = int(round(multiplied_grey));
	
	COLOR.rgb = palette_colours[rounded_grey].rgb;
	
	if (outline_enabled) {
		
	}
}