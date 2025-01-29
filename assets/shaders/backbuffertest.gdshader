shader_type canvas_item;

uniform sampler2D previous_frame; // The backbuffer or previous accumulated texture
uniform vec2 viewport_size;       // The size of the viewport (normalized UV adjustment)

uniform sampler2D SCREEN_TEXTURE: hint_screen_texture, filter_nearest;

void fragment() {
    vec2 uv = UV;

    // Fetch the current pixel color from the viewport (current frame)
    vec4 current_pixel = texture(SCREEN_TEXTURE, SCREEN_UV);

    // Fetch the previously accumulated pixel color from the backbuffer
    vec4 previous_pixel = texture(previous_frame, uv);

    // Accumulate white pixels
    if (current_pixel.r > 0.9 && current_pixel.g > 0.9 && current_pixel.b > 0.9) {
        // If the current pixel is white, retain it
        COLOR = vec4(1.0, 1.0, 1.0, 1.0);
    } else {
        // Otherwise, keep the previous frame's value
        COLOR = previous_pixel;
    }
}