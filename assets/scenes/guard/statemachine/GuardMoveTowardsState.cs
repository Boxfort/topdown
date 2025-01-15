using Godot;
using Godot.Collections;
using System;

public partial class GuardMoveTowardsState : GuardState
{
    Vector2 targetPosition;
    public override void Enter(string previousState, Dictionary data)
    {
        // no-op
        targetPosition = (Vector2)data["position"];
        guard.NavAgent.TargetPosition = targetPosition;
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
        Vector2 direction = Vector2.Zero;

        direction = guard.GlobalPosition.DirectionTo(guard.NavAgent.GetNextPathPosition());
        guard.WeaponContainer.LookAt(guard.Position + direction);
        guard.WeaponContainer.Rotate(Mathf.DegToRad(180));

        if (!guard.NavAgent.IsNavigationFinished())
        {
            guard.SetVelocity(direction * GuardController.Speed);
            guard.MoveAndSlide();
        } else {
            guard.SetVelocity(Vector2.Zero);
            EmitSignal(SignalName.Finished, GuardStates.Idle.ToString(), NO_DATA );
        }

        guard.HandleWalkingAnimation(delta);
    }
}
