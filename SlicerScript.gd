extends CollisionPolygon2D


@export var target: Node2D
@export var tiler: Node2D

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
    pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
    global_position = target.global_position;
    global_rotation = self.global_position.angle_to_point(get_global_mouse_position()) - deg_to_rad(90)

    if (Input.is_action_just_pressed("fire")):
        tiler.carve(self)
