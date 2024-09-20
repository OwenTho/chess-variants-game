shader_type canvas_item;

uniform sampler2D noise : repeat_enable;
uniform sampler2D moving_noise : repeat_enable;
uniform sampler2D colour;

void fragment() {
	float noise_mult = (1. / 100.);
	float move_mult = (1. / 60.);
	vec4 val = texture(noise, vec2(SCREEN_UV.x + sin(TIME * noise_mult), SCREEN_UV.y + cos(TIME * noise_mult))) - texture(moving_noise, vec2(SCREEN_UV.x + TIME * move_mult, SCREEN_UV.y));
	
	COLOR = mix(COLOR, texture(colour, vec2(val.r, 0)), (COLOR.r / 1.5) + ((1.0-COLOR.r) / 1.2));
}