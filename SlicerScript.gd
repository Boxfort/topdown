extends Node2D


@export var camera_shaker: Node2D
@export var fireball: PackedScene
@export var destruction: Node2D

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
    pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:

    var global_mouse_pos = get_viewport().get_canvas_transform().affine_inverse() * get_viewport().get_mouse_position()
    global_rotation = self.global_position.angle_to_point(global_mouse_pos) - deg_to_rad(270)

    if (Input.is_action_just_pressed("fire")):
        var instance: CharacterBody2D = fireball.instantiate()
        var dir = global_position.direction_to(global_mouse_pos)
        get_viewport().add_child(instance);
        instance.add_collision_exception_with(get_parent())
        instance.global_position = global_position
        instance.SetDirection(dir)
        #camera_shaker.ApplyNoiseShake();
