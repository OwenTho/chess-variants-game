shader_type canvas_item;

uniform float progress : hint_range(0.0, 1.0, 0.001) = 0.0;
uniform bool fill_right = true;
uniform vec4 unfinish_mix : source_color;

void fragment() {
	
	float right = float(fill_right);
	float left = float(!fill_right);
	
	float prog_val = progress - (left);
	float compare_val = UV.x * ((left * -1.) + (right * 1.));
	
	if (prog_val < compare_val) {
		COLOR.rgb = mix(COLOR.rgb, unfinish_mix.rgb, 0.4);
	}
}
