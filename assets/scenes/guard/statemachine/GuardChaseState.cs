using Godot;
using Godot.Collections;
using System;

public partial class GuardChaseState : GuardState
{
    public override void Enter(string previousState, Dictionary data)
    {
        // no-op
        player.StoppedBeingDetectedBy(guard);
        guard.NavAgent.MaxSpeed = GuardController.Speed;
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

        if (guard.NavAgent.IsNavigationFinished() || CanAttack(player))
        {
            guard.NavAgent.SetVelocity(Vector2.Zero);
            EmitSignal(SignalName.Finished, GuardStates.Attacking.ToString(), new Godot.Collections.Dictionary() { ["direction"] = direction });
        }
        else
        {
            guard.NavAgent.SetVelocity(direction * GuardController.Speed);
        }

        guard.Velocity = guard.KnockbackVelocity;
        guard.MoveAndSlide();
        guard.HandleSpriteDirection(direction.Angle());
        guard.HandleWalkingAnimation(delta);
    }

    private bool CanAttack(Node2D node)
    {
        return guard.GlobalPosition.DistanceTo(node.GlobalPosition) < GuardController.AttackRange && guard.CanSeeNode(node);
    }
}
