using Godot;
using Godot.Collections;
using System;

public partial class GuardFollowingState : GuardState
{
    public override void Enter(string previousState, Dictionary data)
    {
        // no-op
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

        // TODO: This should be done in a thread on a timer so we're not calculating navigation every step
        //guard.NavAgent.TargetPosition = combinedView.GetGameWorldMousePosition(GetViewport());
        guard.NavAgent.TargetPosition = player.GlobalPosition;

        direction = guard.GlobalPosition.DirectionTo(guard.NavAgent.GetNextPathPosition());
        guard.WeaponContainer.LookAt(guard.Position + direction);
        guard.WeaponContainer.Rotate(Mathf.DegToRad(180));

        if (!guard.NavAgent.IsNavigationFinished())
        {
            guard.SetVelocity(direction * GuardController.Speed);
            guard.MoveAndSlide();
        } else {
            guard.SetVelocity(Vector2.Zero);
            EmitSignal(SignalName.Finished, GuardStates.Attacking.ToString(), new Godot.Collections.Dictionary() { ["direction"] = direction } );
        }

        guard.HandleWalkingAnimation(delta);
    }
}
