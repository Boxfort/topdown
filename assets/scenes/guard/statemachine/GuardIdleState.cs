using Godot;
using Godot.Collections;
using System;

public partial class GuardIdleState : GuardState
{
    public override void Enter(string previousState, Dictionary data)
    {
        // no-op
        guard.QuestionMarkSprite.Animation = "default";
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

    const float detectionRate = 2f;
    const float detectionLossRate = 2;
    const float investigationThreshold = 0.5f;
    float detectionAmount = 0;

    public float DetectionAmount { get => detectionAmount; }

    public override void PhysicsProcess(double delta)
    {
        detectionAmount = guard.HandleDetection(delta, detectionAmount, detectionRate, detectionLossRate, player);
        HandleDetectionDisplay(detectionAmount);

        if (detectionAmount > investigationThreshold)
        {
            EmitSignal(
                SignalName.Finished,
                GuardStates.Investigating.ToString(),
                new Godot.Collections.Dictionary()
                {
                    ["investigation_position"] = player.GlobalPosition,
                    ["initial_position"] = guard.GlobalPosition
                }
            );
        }

        if (guard.KnockbackVelocity != Vector2.Zero)
        {
            guard.Velocity = guard.KnockbackVelocity;
            guard.MoveAndSlide();
        }

        guard.HandleSpriteDirection(guard.LastLookAngle);
    }

    private void HandleDetectionDisplay(float currentDetection)
    {
        if (currentDetection > 0)
        {
            guard.QuestionMarkSprite.Show();
            int frameCount = guard.QuestionMarkSprite.SpriteFrames.GetFrameCount("default");

            int currentFrame = Mathf.CeilToInt(currentDetection * (1 / investigationThreshold) * frameCount) - 1;

            guard.QuestionMarkSprite.SetFrameAndProgress(currentFrame, 0);
        }
        else
        {
            guard.QuestionMarkSprite.Hide();
        }
    }
}
