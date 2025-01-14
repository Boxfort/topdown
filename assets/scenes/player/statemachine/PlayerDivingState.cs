using Godot;
using Godot.Collections;
using System;

public partial class PlayerDivingState : PlayerState
{
    const float diveTime = 0.25f;
    const float diveScaleFactor = 0.5f;
    float diveTimer = 0.0f;
    float slideTimer = 0.0f;
    const float diveInitialSpeed = 200;
    const float diveAirFriction = 10;
    const float diveSlideFriction = 200;
    private const int diveSlideFrictionIncreaseFactor = 10;

    bool hasBonked = false;

    Vector2 direction;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public override void Enter(string previousState, Dictionary data)
    {
        direction = (Vector2)data["direction"];
        player.PlayerSprite.Play("diving");
        player.PlayerSprite.LookAt(player.Position + direction);
        player.PlayerSprite.Rotate(Mathf.DegToRad(90));

        player.Velocity = diveInitialSpeed * direction;
        diveTimer = 0.0f;
        slideTimer = 0.0f;
        hasBonked = false;
    }

    public override void Exit()
    {
        // no-op
    }

    public override void UnhandledInput(InputEvent @event)
    {
        // no-op
    }

    public override void Process(double delta)
    {
        // no-op
    }

    public override void PhysicsProcess(double delta)
    {
        Vector2 velocity = player.Velocity;

        if (diveTimer < diveTime && !hasBonked)
        {
            diveTimer += (float)delta;
            float height = Mathf.Sin((diveTimer * 2 / diveTime) + 1) / 2; // 0 to 1 to 0
            float scale = 1f + (height * diveScaleFactor);
            player.PlayerSprite.Scale = new Vector2(scale, scale);
            velocity = velocity.MoveToward(Vector2.Zero, diveAirFriction * (float)delta);

            KinematicCollision2D collision = player.MoveAndCollide(velocity *(float)delta);

            if (collision != null) 
            {
                velocity = velocity.Bounce(collision.GetNormal())/2;
                player.PlayerSprite.LookAt(player.Position + velocity.Normalized());
                player.PlayerSprite.Rotate(Mathf.DegToRad(90));

                hasBonked = true;
            }
        }
        else if (player.Velocity != Vector2.Zero)
        {
            slideTimer += (float)delta;
            velocity = velocity.MoveToward(Vector2.Zero, diveSlideFriction * Mathf.Max(1, slideTimer * diveSlideFrictionIncreaseFactor) * (float)delta);
            player.MoveAndSlide();
        }
        else
        {
            EmitSignal(SignalName.Finished, PlayerStates.Idle.ToString(), noData);
        }

        player.SetVelocity(this, velocity);
    }
}