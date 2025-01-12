using Godot;
using System;
using System.Linq;

public partial class Fireball : CharacterBody2D
{
    [Signal]
    public delegate void OnCollidedEventHandler();

    AnimatedSprite2D animatedSprite2D;

    public const float Speed = 300.0f;

    bool canExplode = true;

    public override void _Ready()
    {
        animatedSprite2D = GetNode<AnimatedSprite2D>("FireballSprite");
    }

    public void SetDirection(Vector2 direction) 
    {
        Velocity = direction * Speed;
        Rotation = animatedSprite2D.GetAngleTo(GlobalPosition + direction) + Mathf.DegToRad(-90);
    }

    public override void _PhysicsProcess(double delta)
    {
        var collided = MoveAndSlide();

        if (collided && canExplode) 
        {
            Velocity = Vector2.Zero;
            canExplode = false;

            animatedSprite2D.Play("explode");

            TilemapDestructionHandler tilemapDestructionHandler = (TilemapDestructionHandler)GetTree().GetFirstNodeInGroup("destructible_tiles");
            tilemapDestructionHandler.Carve(GetNode<CollisionPolygon2D>("Node2D/CollisionPolygon2D"));

            CameraShaker shaker = (CameraShaker)GetTree().GetFirstNodeInGroup("camera_shaker");
            shaker.ApplyNoiseShake();

            animatedSprite2D.AnimationFinished += async () =>
            {
                QueueFree();
            };
        }
    }
}
