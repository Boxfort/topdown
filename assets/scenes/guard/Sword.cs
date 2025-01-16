using Godot;
using System;

public partial class Sword : Node2D
{
    AnimatedSprite2D swingTrail;
    Sprite2D swordSprite;
    Hitbox hitbox;

    bool isAttacking = false;

    const float swordReturnSpeed = 10f;
    const float attackTime = 0.3f;
    float attackTimer = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        swingTrail = GetNode<AnimatedSprite2D>("SwingTrail");
        swordSprite = GetNode<Sprite2D>("SwordSprite");
        hitbox = GetNode<Hitbox>("Hitbox");

        hitbox.HitboxEntered += OnHitboxEntered;
    }

    private void OnHitboxEntered(Hurtbox hurtbox)
    {
        hurtbox.OnHit(new() { damage = 1, fromPosition = GlobalPosition, knockbackForce = 100 });
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (isAttacking)
        {
            attackTimer += (float)delta;

            if (attackTimer >= attackTime)
            {
                isAttacking = false;
                attackTimer = 0;
            }
        }
        else
        {
            swordSprite.Rotation = Mathf.LerpAngle(swordSprite.Rotation, 0, (float)delta * swordReturnSpeed);
            hitbox.SetCollisionDisabled(true);
        }
    }

    public void PlayAttack()
    {
        attackTimer = 0;
        swordSprite.RotationDegrees = -156;
        swingTrail.Animation = "attack";
        swingTrail.SetFrameAndProgress(0, 0);
        swingTrail.Play();
        isAttacking = true;
        hitbox.SetCollisionDisabled(false);
    }
}
