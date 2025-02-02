shader_type canvas_item;

uniform sampler2D mask_texture: filter_nearest;
uniform sampler2D over_mask_texture: filter_nearest;
uniform sampler2D under_mask_texture: filter_nearest;
uniform sampler2D fog_of_war_texture: filter_nearest;

// The current position of the camera in pixels
uniform vec2 camera_position = vec2(0,0);

// The size of the fog of war texture in pixels, larger than the window size
uniform vec2 fog_of_war_size = vec2(0,0);

// The current window size in pixels
uniform vec2 window_size = vec2(0,0);

uniform bool weighted = true;

void fragment() {
    vec4 color = texture(TEXTURE, UV);
	vec4 mask = texture(mask_texture, UV);
    vec4 underMask = texture(under_mask_texture, UV);
    vec4 overMask = texture(over_mask_texture, UV);

    vec2 fogUV = ((UV*window_size)+camera_position-(window_size/2.0))/fog_of_war_size;
    fogUV = clamp(fogUV, vec2(0.0), vec2(1.0));
    vec4 fogOfWar = texture(fog_of_war_texture, fogUV);

	if(mask.r > 0.5) {
        if (underMask.a == 0.0) {
            color = overMask;
        } else {
            // Check to see if the position in the fog of war texture has been set
            if (fogOfWar.a > 0.0) {
                vec3 greyUnder = mix(vec3((underMask.r + underMask.g + underMask.b) / 3.0),  vec3(0.299 * underMask.r + 0.587 * underMask.g + 0.114 * underMask.b), float(weighted));
                color.rgb = greyUnder;
                color.a = underMask.a;
            } else {
                color.rgb = vec3(0,0,0);
            }
        }
	} else {
        color = overMask;
    }
	COLOR = color;
}