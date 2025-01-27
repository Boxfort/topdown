using Godot;
using Godot.Collections;
using System;

public partial class GuardAlertState : GuardState
{
    Vector2 velocity = Vector2.Zero;
    const float alertedInitialVelocity = 50f;
    const float alertedVelocityFriction = 150f;

    const float alertedTime = 0.4f;
    float alertedTimer = 0;

    public override void Enter(string previousState, Dictionary data)
    {
        guard.AlertAudio.Play();
        guard.ExclaimationMarkSprite.Show();
        guard.QuestionMarkSprite.Hide();
        var direction = (Vector2)data["direction"];
        velocity = alertedInitialVelocity * direction;
        guard.SetVelocity(velocity);
    }

    public override void Exit()
    {
        guard.QuestionMarkSprite.Hide();
        guard.ExclaimationMarkSprite.Hide();
    }

    public override void PhysicsProcess(double delta)
    {
        alertedTimer += (float)delta;

        velocity = velocity.MoveToward(Vector2.Zero, alertedVelocityFriction * (float)delta);
        guard.SetVelocity(velocity + guard.KnockbackVelocity);
        guard.MoveAndSlide();
        guard.GuardSprite.Rotation = 0;

        if (alertedTimer >= alertedTime)
        {
            EmitSignal(
                SignalName.Finished,
                GuardStates.Chase.ToString(),
                NO_DATA
            );
        }
    }

    public override void Process(double delta)
    {
        // no-op
    }

    public override void UnhandledInput(InputEvent @event)
    {
        // no-op
    }
}
