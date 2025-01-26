using Godot;
using System;

public partial class Sword : Node2D
{
    AnimatedSprite2D swingTrail;
    Sprite2D swordSprite;
    Hitbox hitbox;
    AudioStreamPlayer2DCustom swingAudio;
    AudioStreamPlayer2DCustom hitAudio;

    bool isAttacking = false;

    const float swordReturnSpeed = 10f;
    const float attackTime = 0.3f;
    float attackTimer = 0;

    public void SetFlipV(bool isFlipped)
    {
        swordSprite.FlipV = isFlipped;
        swingTrail.FlipV = isFlipped;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        swingTrail = GetNode<AnimatedSprite2D>("SwingTrail");
        swordSprite = GetNode<Sprite2D>("SwordSprite");
        hitbox = GetNode<Hitbox>("Hitbox");
        swingAudio = GetNode<AudioStreamPlayer2DCustom>("SwingAudio");
        hitAudio = GetNode<AudioStreamPlayer2DCustom>("HitAudio");

        hitbox.HitboxEntered += OnHitboxEntered;
    }

    private void OnHitboxEntered(Hurtbox hurtbox)
    {
        hitAudio.Play();
        hurtbox.OnHit(new() { damage = 1, fromPosition = GlobalPosition, knockbackForce = 100 });
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (GlobalRotationDegrees > 90 || GlobalRotationDegrees < -90)
        {
            SetFlipV(true);
        }
        else
        {
            SetFlipV(false);
        }

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
        swingAudio.Play();
        attackTimer = 0;
        swordSprite.RotationDegrees = -156;
        swingTrail.Animation = "attack";
        swingTrail.SetFrameAndProgress(0, 0);
        swingTrail.Play();
        isAttacking = true;
        hitbox.SetCollisionDisabled(false);
    }
}
