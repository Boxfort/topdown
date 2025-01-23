using Godot;
using Godot.Collections;
using System;

public partial class GuardPatrolState : GuardState
{
    int currentPathNodeIdx = 0;

    bool isWaiting = false;
    float currentWaitTime = 0;
    float currentWaitTimer = 0;
    Vector2 waitFacingDirection = Vector2.Zero;

    public override void Enter(string previousState, Dictionary data)
    {
        isWaiting = false;
        currentWaitTime = 0;
        currentWaitTimer = 0;
        guard.NavAgent.TargetPosition = guard.PatrolPath.GetNodeAtIdx(currentPathNodeIdx).GlobalPosition;
    }

    public override void Exit()
    {
        // no-op
    }

    public override void PhysicsProcess(double delta)
    {
        Vector2 direction = Vector2.Zero;

        direction = guard.GlobalPosition.DirectionTo(guard.NavAgent.GetNextPathPosition());

        if (isWaiting)
        {
            currentWaitTimer += (float)delta;
            direction = waitFacingDirection;

            if (currentWaitTimer >= currentWaitTime)
            {
                isWaiting = false;
                currentWaitTime = 0;
                currentWaitTimer = 0;
                GoToNextPathNode();
            }
        }
        else
        {
            if (!guard.NavAgent.IsNavigationFinished())
            {
                guard.SetVelocity(direction * GuardController.Speed / 2 + guard.KnockbackVelocity);
                guard.MoveAndSlide();
            }
            else
            {
                PathNode currentNode = guard.PatrolPath.GetNodeAtIdx(currentPathNodeIdx);
                if (currentNode.ShouldFaceDirection)
                {
                    waitFacingDirection = currentNode.Direction;
                }

                if (currentNode.ShouldWait)
                {
                    isWaiting = true;
                    currentWaitTime = currentNode.WaitTime;
                    guard.SetVelocity(Vector2.Zero);
                }
                else
                {
                    GoToNextPathNode();
                }
            }
        }

        guard.WeaponContainer.LookAt(guard.Position + direction);
        guard.WeaponContainer.Rotate(Mathf.DegToRad(180));

        guard.HandleSpriteDirection(direction.Angle());
        guard.HandleWalkingAnimation(delta);
    }

    private void GoToNextPathNode()
    {
        currentPathNodeIdx = (currentPathNodeIdx + 1) % guard.PatrolPath.PathNodeCount;
        guard.NavAgent.TargetPosition = guard.PatrolPath.GetNodeAtIdx(currentPathNodeIdx).GlobalPosition;
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
