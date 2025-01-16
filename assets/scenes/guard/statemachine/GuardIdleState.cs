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

    public override void PhysicsProcess(double delta)
    {
        if (guard.CanSeeNode(player))
        {
            if (player.CurrentLightValue > 0.25f)
            {
                // TODO: vary by distance to target
                var distanceFactor = 1 - Mathf.Clamp(Mathf.Sqrt(guard.GlobalPosition.DistanceTo(player.GlobalPosition)-64) / Mathf.Sqrt(GuardController.DetectionRadius), 0, 1);
                //distance
                detectionAmount += (float)delta * detectionRate * distanceFactor * player.CurrentLightValue;

                if (detectionAmount > investigationThreshold)
                {
                    EmitSignal(
                        SignalName.Finished, 
                        GuardStates.Investigating.ToString(), 
                        new Godot.Collections.Dictionary() { 
                            ["investigation_position"] = player.GlobalPosition, 
                            ["initial_position"] = guard.GlobalPosition 
                        }
                    );
                }
            }
            else
            {
                HandleDecreaseDetection(delta);
            }
        }
        else
        {
            HandleDecreaseDetection(delta);
        }

        HandleDetectionDisplay();
    }

    private void HandleDecreaseDetection(double delta)
    {
        if (detectionAmount > 0)
        {
            detectionAmount = Mathf.Max(0, detectionAmount - ((float)delta * detectionLossRate));
        }
    }

    private void HandleDetectionDisplay()
    {
        if (detectionAmount > 0)
        {
            guard.QuestionMarkSprite.Show();
            int frameCount = guard.QuestionMarkSprite.SpriteFrames.GetFrameCount("default");

            int currentFrame = Mathf.CeilToInt(detectionAmount * (1 / investigationThreshold) * frameCount) - 1;

            guard.QuestionMarkSprite.SetFrameAndProgress(currentFrame, 0);
        }
        else
        {
            guard.QuestionMarkSprite.Hide();
        }
    }
}
