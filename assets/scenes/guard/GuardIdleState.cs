using Godot;
using Godot.Collections;
using System;

public partial class GuardIdleState : GuardState
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
        // no-op
        if (guard.GlobalPosition.DistanceTo(player.GlobalPosition) <= GuardController.DetectionRadius) 
        {
            var spaceState = GetViewport().GetWorld2D().DirectSpaceState;
            var query = PhysicsRayQueryParameters2D.Create(guard.GlobalPosition, player.GlobalPosition, 0b0000_0110);
            var result = spaceState.IntersectRay(query);

            if (result.Count > 0) {
                Node2D collidedNode= (Node2D)result["collider"];
                if (collidedNode is PlayerController player) {
                    EmitSignal(SignalName.Finished, GuardStates.Following.ToString(), NO_DATA);
                }
            }

        }
    }
}
