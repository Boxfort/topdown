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
        guard.NavAgent.PathDesiredDistance = 1;
        guard.NavAgent.TargetDesiredDistance = 1;
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

        // TODO: This should be done in a thread on a timer so we're not calculating navigation every step
        if (guard.CanSeeNode(player))
        {
            guard.PlayerLastLocationMarker.Hide();
            guard.NavAgent.TargetPosition = player.GlobalPosition;
        }
        else
        {
            // TODO: This sucks, have it only trigger once
            guard.PlayerLastLocationMarker.Show();
            guard.PlayerLastLocationMarker.GlobalPosition = guard.NavAgent.TargetPosition;
            guard.NavAgent.TargetPosition = guard.PlayerLastLocationMarker.GlobalPosition;
        }

        if (guard.NavAgent.IsNavigationFinished())
        {
            if (!guard.CanSeeNode(player))
            {
                guard.PlayerLastLocationMarker.Hide();
                EmitSignal(SignalName.Finished, GuardStates.Patrol.ToString(), NO_DATA);
            }
            else if (CanAttack(player))
            {
                guard.NavAgent.SetVelocity(Vector2.Zero);
                EmitSignal(SignalName.Finished, GuardStates.Attacking.ToString(), new Godot.Collections.Dictionary() { ["direction"] = direction });
            }
        }
        else
        {
            guard.NavAgent.SetVelocity(direction * GuardController.Speed);
        }

        guard.HandleSpriteDirection(direction.Angle());
        guard.HandleWalkingAnimation(delta);
        guard.Velocity = guard.KnockbackVelocity;
        guard.MoveAndSlide();
    }

    private bool CanAttack(Node2D node)
    {
        return guard.GlobalPosition.DistanceTo(node.GlobalPosition) < GuardController.AttackRange && guard.CanSeeNode(node);
    }
}
