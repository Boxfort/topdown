using Godot;
using System;

public partial class PlayerIdleState : PlayerState
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
    }

    public override void Enter(string previousState, Godot.Collections.Dictionary data)
    {
        // no-op
        player.PlayerSprite.Play("idle");
        player.Velocity = Vector2.Zero;
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
        player.HandleWalkingAnimation(delta);
    }

    public override void PhysicsProcess(double delta)
    {
        Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        if (direction != Vector2.Zero) EmitSignal(State.SignalName.Finished, PlayerStates.Running.ToString(), noData);
    }
}
