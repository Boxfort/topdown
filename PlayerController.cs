using Godot;
using System;

public partial class PlayerController : CharacterBody2D
{
    public const float speed = 50.0f;
    public const float friction = 15.0f;

    Sprite2D playerSprite;

    public override void _Ready()
    {
        playerSprite = GetNode<Sprite2D>("PlayerSprite");
    }

    public const float jiggleSpeed = 20.0f;
    double deltaCount = 0;

    public override void _Process(double delta)
    {
        deltaCount = (deltaCount + (delta * jiggleSpeed)) % 100;

        if (Velocity != Vector2.Zero) 
        {
            // a is -1 to 1
            float a = (float)Mathf.Sin(deltaCount);
            GD.Print(deltaCount);

            float rot = a * 10f;

            playerSprite.RotationDegrees = rot;
        } else {
            playerSprite.RotationDegrees = Mathf.Lerp(playerSprite.RotationDegrees, 0, (float)delta * jiggleSpeed);
        }
    }

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
