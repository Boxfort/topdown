using Godot;
using Godot.Collections;
using System;

public partial class GuardInvestigatingState : GuardState
{
    Vector2 investigationPosition;
    Vector2 initialPosition;

    public override void Enter(string previousState, Dictionary data)
    {
        investigationPosition = (Vector2)data["investigation_position"];
        initialPosition = (Vector2)data["initial_position"];
        guard.NavAgent.TargetPosition = investigationPosition;
        investigateTimer = 0;
        alerted = false;
        alertedTimer = 0;

        guard.QuestionMarkSprite.Show();
        guard.QuestionMarkSprite.Animation = "warning";
    }

    public override void Exit()
    {
        guard.QuestionMarkSprite.Hide();
        guard.ExclaimationMarkSprite.Hide();
        // no-op
    }

    public override void Process(double delta)
    {
        // no-op
    }

    public override void UnhandledInput(InputEvent @event)
    {
        // no-op
    }

    const float detectionRate = 2f;
    const float detectionLossRate = 1;
    const float minimumDetection = 0.5f;
    const float discoveryThreshold = 1.5f;
    float detectionAmount = minimumDetection;

    const float investigateTime = 0.5f;
    float investigateTimer = 0.5f;

    const float alertedTime = 0.4f;
    float alertedTimer = 0;
    bool alerted = false;
    const float alertedInitialVelocity = 50f;
    const float alertedVelocityFriction = 150f;

    Vector2 velocity = Vector2.Zero;

    public override void PhysicsProcess(double delta)
    {
        if (alerted)
        {
            alertedTimer += (float)delta;

            velocity = velocity.MoveToward(Vector2.Zero, alertedVelocityFriction * (float)delta);
            guard.SetVelocity(velocity);
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
        else
        {
            if (!guard.NavAgent.IsNavigationFinished())
            {
                Vector2 direction = guard.GlobalPosition.DirectionTo(guard.NavAgent.GetNextPathPosition());
                guard.WeaponContainer.LookAt(guard.Position + direction);
                guard.WeaponContainer.Rotate(Mathf.DegToRad(180));
                velocity = (direction * GuardController.Speed / 2) + guard.KnockbackVelocity;
                guard.SetVelocity(velocity);
                guard.MoveAndSlide();
            }
            else
            {
                investigateTimer += (float)delta;
                if (investigateTimer >= investigateTime)
                {
                    investigateTimer = 0;
                    EmitSignal(
                        SignalName.Finished,
                        GuardStates.MoveTowards.ToString(),
                        new Godot.Collections.Dictionary()
                        {
                            ["position"] = initialPosition,
                        }
                    );
                }
            }

            guard.HandleWalkingAnimation(delta);

            if (guard.CanSeeNode(player))
            {
                detectionAmount += (float)delta * detectionRate * (player.CurrentLightValue + 0.1f);

                if (detectionAmount > discoveryThreshold)
                {
                    guard.ExclaimationMarkSprite.Show();
                    guard.QuestionMarkSprite.Hide();
                    alerted = true;
                    velocity = (alertedInitialVelocity * player.GlobalPosition.DirectionTo(guard.GlobalPosition)) + guard.KnockbackVelocity;
                    guard.SetVelocity(velocity);
                }
            }
            else
            {
                HandleDecreaseDetection(delta);
            }
        }

        HandleDetectionDisplay();
    }

    private void HandleDecreaseDetection(double delta)
    {
        if (detectionAmount > minimumDetection)
        {
            detectionAmount = Mathf.Max(minimumDetection, detectionAmount - ((float)delta * detectionLossRate));
        }
    }

    private void HandleDetectionDisplay()
    {
        if (detectionAmount > minimumDetection)
        {
            int frameCount = guard.QuestionMarkSprite.SpriteFrames.GetFrameCount("default");

            // This math is doo doo and not flexible
            var normalisedDetection = detectionAmount - minimumDetection;

            int currentFrame = Mathf.CeilToInt(normalisedDetection * frameCount) - 1;

            guard.QuestionMarkSprite.SetFrameAndProgress(currentFrame, 0);
        }
    }
}
