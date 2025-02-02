shader_type canvas_item;

uniform sampler2D mask_texture: filter_nearest;
uniform vec3 mask_position;
uniform vec2 mask_size;
uniform vec4 reset_color: source_color;


void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
    vec4 mask = texture(mask_texture, UV);

    if (mask == reset_color) {
        mask.a = 0.0;
    } else {
       	if(mask.r > 0.5) {
            mask.a = 0.0;
   	    } else {
            mask.rgb = vec3(1,1,1);
        }
    }

    COLOR = mask;
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
