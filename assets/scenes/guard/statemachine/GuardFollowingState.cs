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

    /// <summary>
    /// Gets the mouse position in the game world.
    /// This function is necessary as the actual rendered image on screen does not corellate 1 to 1 with the game world
    /// </summary>
    /// <returns></returns>
    private Vector2 GetGameWorldMousePosition()
    {
        Vector2 viewportMousePos = GetViewport().CanvasTransform.AffineInverse() * GetViewport().GetMousePosition();
        Vector2 cameraPosition = GetViewport().GetCamera2D().Position;

        return (viewportMousePos + (cameraPosition * (combinedView.Scale.X - 1))) / combinedView.Scale.X;
    }

    public override void PhysicsProcess(double delta)
    {
        Vector2 direction = Vector2.Zero;

        guard.NavAgent.TargetPosition = GetGameWorldMousePosition();

        direction = guard.GlobalPosition.DirectionTo(guard.NavAgent.GetNextPathPosition());

        if (!guard.NavAgent.IsNavigationFinished())
        {
            guard.SetVelocity(direction * GuardController.Speed);

            guard.MoveAndSlide();

            guard.HandleWalkingAnimation(delta);
        }
    }
}
