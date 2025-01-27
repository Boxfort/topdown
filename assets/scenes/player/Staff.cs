using Godot;
using System;

public partial class Staff : Node2D
{
    Sprite2D staffSprite;
    AnimatedSprite2D swingTrail;
    AudioStreamPlayer2DCustom swingAudio;
    AudioStreamPlayer2DCustom hitAudio;
    Hitbox hitbox;

    bool isAttacking = false;
    const float staffReturnSpeed = 10f;
    const float attackTime = 0.3f;
    float attackTimer = 0;
    Vector2 staffOffset = new Vector2(-5, 0);
    Vector2 staffAttackOffset = new Vector2(-14, 0);

    public bool IsAttacking { get => isAttacking; }

    public void SetFlipV(bool isFlipped)
    {
        staffSprite.FlipV = isFlipped;
        swingTrail.FlipV = isFlipped;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        swingTrail = GetNode<AnimatedSprite2D>("SwingTrail");
        staffSprite = GetNode<Sprite2D>("StaffSprite");
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
            staffSprite.Rotation = Mathf.LerpAngle(staffSprite.Rotation, 0, (float)delta * staffReturnSpeed);
            staffSprite.Position = staffSprite.Position.Lerp(staffOffset, (float)delta * staffReturnSpeed);
            hitbox.SetCollisionDisabled(true);
        }
    }

    public void PlayAttack()
    {
        attackTimer = 0;
        staffSprite.RotationDegrees = 156 * (staffSprite.FlipV ? 1 : -1);
        staffSprite.Position = staffAttackOffset;
        swingTrail.Animation = "attack";
        swingTrail.SetFrameAndProgress(0, 0);
        swingTrail.Play();
        swingAudio.Play();
        isAttacking = true;
        hitbox.SetCollisionDisabled(false);
    }
}
