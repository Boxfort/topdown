using Godot;
using Godot.Collections;
using System;

public partial class GuardPatrolState : GuardState
{
    int currentPathNodeIdx = 0;
    public override void Enter(string previousState, Dictionary data)
    {
        guard.NavAgent.TargetPosition = guard.PatrolPath.Curve.GetPointPosition(currentPathNodeIdx) + guard.PatrolPath.GlobalPosition;
    }

    public override void Exit()
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
            guard.SetVelocity(direction * GuardController.Speed/2 + guard.KnockbackVelocity);
            guard.MoveAndSlide();
        } else {
            currentPathNodeIdx = (currentPathNodeIdx + 1) % guard.PatrolPath.Curve.PointCount;
            guard.NavAgent.TargetPosition = guard.PatrolPath.Curve.GetPointPosition(currentPathNodeIdx) + guard.PatrolPath.GlobalPosition;
        }

        guard.HandleSpriteDirection(direction.Angle());
        guard.HandleWalkingAnimation(delta);
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
