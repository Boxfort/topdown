using Godot;
using System;

public partial class PlayerController : CharacterBody2D
{
    public const float speed = 100.0f;
    public const float friction = 250.0f;

    AnimatedSprite2D playerSprite;
    public AnimatedSprite2D PlayerSprite { get => playerSprite; }

    public override void _Ready()
    {
        playerSprite = GetNode<AnimatedSprite2D>("PlayerSprite");
    }

    public override void _Process(double delta)
    {

    }

    public void SetVelocity(State from, Vector2 v) {
        // We have to check this because of a race condition with multiple states calling Process/PhysicsProcess at the same time.
        if (from.hasExited) return;
        Velocity = v;
    }

    public const float jiggleSpeed = 20.0f;
    float deltaCount = 0;

    public void HandleWalkingAnimation(double delta)
    {
        deltaCount = (deltaCount + (float)(delta * jiggleSpeed)) % 100;

        if (Velocity != Vector2.Zero) 
        {
            float spriteRotation = Mathf.Sin(deltaCount) * 10f;
            playerSprite.RotationDegrees = spriteRotation;
        } else {
            playerSprite.RotationDegrees = Mathf.Lerp(playerSprite.RotationDegrees, 0, (float)delta * jiggleSpeed);
        }
    }

    public override void _PhysicsProcess(double delta)
    {

    }
}
