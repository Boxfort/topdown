shader_type canvas_item;

uniform sampler2D dissolve_texture;
uniform float dissolve_value : hint_range(0,1);
uniform sampler2D SCREEN_TEXTURE: hint_screen_texture, filter_nearest;

void fragment(){
    vec4 main_texture = texture(TEXTURE, UV) * COLOR;
    vec4 noise_texture = texture(dissolve_texture, UV);
    main_texture.a *= floor(dissolve_value + min(1, noise_texture.x));
    COLOR = main_texture;
}