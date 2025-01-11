shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;
uniform sampler2D mask_texture;

uniform float brightness = 1.0;
uniform float contrast = 1.0;
uniform float saturation = 1.0;

void fragment() {
    vec4 color = texture(TEXTURE, UV);
	vec4 mask = texture(mask_texture, UV);
	if(mask.r > 0.5) {
		color *= 0.0;
	}
	COLOR = color;
}