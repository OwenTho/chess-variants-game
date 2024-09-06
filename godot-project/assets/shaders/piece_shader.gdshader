shader_type canvas_item;

// Palette settings
// Implemented from https://godotshaders.com/shader/extensible-color-palette/ (CC0)
uniform int palette_count : hint_range(1, 10, 1);
uniform vec4 palette_colours[3] : source_color;

// Outline settings
// Implemented from https://godotshaders.com/shader/outline-that-disrespects-boundaries/ (MIT)
uniform bool outline_margin = true;
uniform bool outline_enabled = false;
uniform float outline_size : hint_range(1, 10, 0.1) = 2;
uniform vec4 outline_color : source_color;

// Needs:
// - Optional outline

void vertex() {
	if (outline_margin) {
		VERTEX += (UV * 2.0 - 1.0) * outline_size;
	}
}

void fragment() {
	vec2 uv = UV;
	if (outline_margin) {
		vec2 pixel_size = vec2(1.) / (vec2(1.) / TEXTURE_PIXEL_SIZE + vec2(outline_size * 2.));
		uv = (uv - pixel_size * outline_size) * TEXTURE_PIXEL_SIZE / pixel_size;
		if (uv != clamp(uv, vec2(0.), vec2(1.))) {
			COLOR.a = 0.0;
		} else {
			COLOR = texture(TEXTURE, uv);
		}
	} else {
		COLOR = texture(TEXTURE, uv);
	}
	float grey_value = max(COLOR.r, COLOR.g);
	grey_value = max(grey_value, COLOR.b);
	
	float multiplier = float(palette_colours.length()-1);
	float multiplied_grey = grey_value * multiplier;
	
	int rounded_grey = int(round(multiplied_grey));
	
	COLOR = vec4(palette_colours[rounded_grey].rgb, COLOR.a);
	
	if (outline_enabled) {
		vec2 size = TEXTURE_PIXEL_SIZE * outline_size;
		
		if (outline_size >= 0.1) {
			float outline = texture(TEXTURE, uv + vec2(-size.x, 0)).a;
			outline += texture(TEXTURE, uv + vec2(0, size.y)).a;
	        outline += texture(TEXTURE, uv + vec2(size.x, 0)).a;
	        outline += texture(TEXTURE, uv + vec2(0, -size.y)).a;
	        outline += texture(TEXTURE, uv + vec2(size.x, size.y)).a;
	        outline += texture(TEXTURE, uv + vec2(size.x, -size.y)).a;
	        outline += texture(TEXTURE, uv + vec2(-size.x, size.y)).a;
	        outline += texture(TEXTURE, uv + vec2(-size.x, -size.y)).a;
	        outline = min(outline, 1.0);
			
			vec4 color = COLOR;
			COLOR = mix(COLOR, outline_color, outline - color.a);
		}
	}
}