shader_type canvas_item;

uniform float progress : hint_range(0.0, 1.0, 0.001) = 0.0;
uniform float angle = 5.;
uniform bool fill_right = true;
uniform vec4 unfinish_mix : source_color;

void vertex() {
	// Called for every vertex the material is visible on.
	/*float val_right = float(fill_right);
	float offset = val_right * 2.;
	offset -= 1.;
	
	float half = step(UV.x, 0.5);
	half -= 1.;
	half *= -1.;
	
	VERTEX.x -= UV.y * offset * angle - half * 1. * offset * angle;*/
	float angle_right = float(fill_right);
	float angle_left = 1.- angle_right;
	
	float upper_y = step(UV.y, 0.5);
	float lower_y = 1. - upper_y;
	
	float on_left = step(UV.x, 0.5);
	float on_right = 1. - on_left;
	
	float offset = 0.;
	// Top left (Fill right)
	offset += on_left * upper_y * angle_right;
	// Bottom left (Fill left)
	offset += on_left * lower_y * angle_left;
	// Top left (Fill right)
	offset -= on_right * upper_y * angle_left;
	// Bottom left (Fill left)
	offset -= on_right * lower_y * angle_right;
	
	VERTEX.x += offset * angle;
}

void fragment() {
	
	float right = float(fill_right);
	float left = 1. - right;
	
	float prog_val = progress - (left);
	float compare_val = UV.x * ((left * -1.) + (right * 1.));
	
	if (prog_val < compare_val) {
		COLOR.rgb = mix(COLOR.rgb, unfinish_mix.rgb, 0.4);
	}
}
