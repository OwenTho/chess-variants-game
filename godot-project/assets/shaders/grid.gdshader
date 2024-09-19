shader_type canvas_item;

uniform sampler2D noise : repeat_enable;
uniform sampler2D moving_noise : repeat_enable;
uniform sampler2D colour;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec4 val = texture(noise, SCREEN_UV) - texture(moving_noise, vec2(SCREEN_UV.x + (TIME * (1. / 40.)), SCREEN_UV.y));
	
	COLOR = mix(COLOR, texture(colour, vec2(val.r, 0)), (COLOR.r / 1.5) + ((1.0-COLOR.r) / 1.2));
}