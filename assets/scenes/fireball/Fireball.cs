using Godot;
using System;
using System.Linq;

public partial class Fireball : CharacterBody2D
{
    [Signal]
    public delegate void OnCollidedEventHandler();

    AnimatedSprite2D animatedSprite2D;
    Hitbox hitbox;

    public const float Speed = 300.0f;

    bool canExplode = true;

    public override void _Ready()
    {
        animatedSprite2D = GetNode<AnimatedSprite2D>("FireballSprite");
        hitbox = GetNode<Hitbox>("Hitbox");
        hitbox.HitboxEntered += OnHitboxEntered;
        hitbox.SetCollisionDisabled(true);
    }

    private void OnHitboxEntered(Hurtbox hurtbox)
    {
        hurtbox.OnHit(new() { damage = 1, fromPosition = GlobalPosition, knockbackForce = 100 });
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
            hitbox.SetCollisionDisabled(false);
            Velocity = Vector2.Zero;
            canExplode = false;

            animatedSprite2D.Play("explode");

            TilemapDestructionHandler tilemapDestructionHandler = (TilemapDestructionHandler)GetTree().GetFirstNodeInGroup("destructible_tiles");
            tilemapDestructionHandler.Carve(GetNode<CollisionPolygon2D>("TerrainDestructionPolygon/CollisionPolygon2D"));

            CameraShaker shaker = (CameraShaker)GetTree().GetFirstNodeInGroup("camera_shaker");
            shaker.ApplyNoiseShake();

            CollisionLayer = 0;

            animatedSprite2D.AnimationFinished += async () =>
            {
                QueueFree();
            };
        }
    }
}
