shader_type canvas_item;

uniform sampler2D mask_texture;
uniform sampler2D under_mask_texture;

void fragment() {
    vec4 color = texture(TEXTURE, UV);
	vec4 mask = texture(mask_texture, UV);
    vec4 underMask = texture(under_mask_texture, UV);
	if(mask.r > 0.5) {
		color = underMask;
	}
	COLOR = color;
}