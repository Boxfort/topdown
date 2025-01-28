using Godot;
using Godot.Collections;
using System;

public partial class GuardInvestigatingState : GuardState
{
    Vector2 investigationPosition;
    Vector2 initialPosition;

    string previousState = "";

    public override void Enter(string previousState, Dictionary data)
    {
        // FIXME: HACK, do this by passing in data, something like 'dont_set_previous_state'?
        if (previousState != "Attacking" && previousState != "Investigating")
        {
            this.previousState = previousState;
        }

        if (data.ContainsKey("investigation_position"))
        {
            investigationPosition = (Vector2)data["investigation_position"];
        }
        if (data.ContainsKey("initial_position"))
        {
            initialPosition = (Vector2)data["initial_position"];
        }

        guard.NavAgent.TargetPosition = investigationPosition;
        investigateTimer = 0;
        investigateStartTimer = 0;

        guard.QuestionMarkSprite.Show();
        guard.QuestionMarkSprite.Animation = "warning";
        guard.NavAgent.MaxSpeed = GuardController.Speed / 2;
        guard.NavAgent.TargetDesiredDistance = 32;
    }

    public override void Exit()
    {
        guard.QuestionMarkSprite.Hide();
        guard.ExclaimationMarkSprite.Hide();
        guard.NavAgent.TargetDesiredDistance = 1;
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
    const float minimumDetection = 0f;
    const float discoveryThreshold = 1f;
    float detectionAmount = minimumDetection;

    const float investigateTime = 0.5f;
    float investigateTimer = 0.5f;
    const float investigateStartTime = 0.5f;
    float investigateStartTimer = 0.5f;

    Vector2 velocity = Vector2.Zero;

    public override void PhysicsProcess(double delta)
    {
        Vector2 direction = guard.GlobalPosition.DirectionTo(guard.NavAgent.GetNextPathPosition());
        guard.WeaponContainer.LookAt(guard.Position + direction);
        guard.WeaponContainer.Rotate(Mathf.DegToRad(180));
        guard.HandleSpriteDirection(direction.Angle());

        if (!guard.NavAgent.IsNavigationFinished())
        {
            if (investigateStartTimer < investigateStartTime)
            {
                investigateStartTimer += (float)delta;
            }
            else
            {
                velocity = (direction * GuardController.Speed / 2) + guard.KnockbackVelocity;
                guard.NavAgent.SetVelocity(velocity);
            }
        }
        else
        {
            guard.NavAgent.SetVelocity(Vector2.Zero);
            investigateTimer += (float)delta;
            if (investigateTimer >= investigateTime)
            {
                investigateTimer = 0;
                EmitSignal(
                    SignalName.Finished,
                    previousState,
                    NO_DATA
                );
            }
        }


        detectionAmount = guard.HandleDetection(delta, detectionAmount, detectionRate, detectionLossRate, player);

        if (guard.GlobalPosition.DistanceTo(player.GlobalPosition) < 32 && player.CurrentLightValue >= 0.5f)
        {
            detectionAmount = discoveryThreshold;
        }

        if (detectionAmount >= discoveryThreshold)
        {
            EmitSignal(
                SignalName.Finished,
                GuardStates.Alert.ToString(),
                new Godot.Collections.Dictionary()
                {
                    ["direction"] = player.GlobalPosition.DirectionTo(guard.GlobalPosition),
                }
            );

        }

        guard.HandleWalkingAnimation(delta);

        HandleDetectionDisplay();
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
