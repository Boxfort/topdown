shader_type canvas_item;

uniform sampler2D mask_texture: filter_nearest;
uniform sampler2D over_mask_texture: filter_nearest;
uniform sampler2D under_mask_texture: filter_nearest;

uniform bool weighted = true;

void fragment() {
    vec4 color = texture(TEXTURE, UV);
	vec4 mask = texture(mask_texture, UV);
    vec4 underMask = texture(under_mask_texture, UV);
    vec4 overMask = texture(over_mask_texture, UV);
	if(mask.r > 0.5) {
        if (underMask.a == 0.0) {
            color = overMask;
        } else {
            vec3 greyUnder = mix(vec3((underMask.r + underMask.g + underMask.b) / 3.0),  vec3(0.299 * underMask.r + 0.587 * underMask.g + 0.114 * underMask.b), float(weighted));
            color.rgb = greyUnder;
            color.a = underMask.a;
        }
	} else {
        color = overMask;
    }
	COLOR = color;
}