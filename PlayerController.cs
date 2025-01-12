using Godot;
using System;

public partial class PlayerController : CharacterBody2D
{
    public const float speed = 100.0f;
    public const float friction = 15.0f;


    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        if (direction != Vector2.Zero)
        {
            velocity = direction * speed;
        }
        else
        {
            velocity = velocity.MoveToward(Vector2.Zero, friction);
        }

        Velocity = velocity;
        MoveAndSlide();
    }
}
