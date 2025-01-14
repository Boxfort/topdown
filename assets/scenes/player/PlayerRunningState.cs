using Godot;
using Godot.Collections;
using System;

public partial class PlayerRunningState : PlayerState
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public override void Enter(string previousState, Dictionary data)
    {
        // no-op
        player.PlayerSprite.Play("idle");
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
        player.HandleWalkingAnimation(delta);
    }

    public override void PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("jump") && player.Velocity != Vector2.Zero)
        {
            Dictionary data = new()
            {
                ["direction"] = player.Velocity.Normalized()
            };
            EmitSignal(State.SignalName.Finished, PlayerStates.Diving.ToString(), data);
        }

        HandleMovement(delta);
    }

    private void HandleMovement(double delta)
    {
        Vector2 velocity = player.Velocity;
        Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");

        if (direction != Vector2.Zero)
        {
            velocity = direction * PlayerController.speed;
        }
        else
        {
            velocity = velocity.MoveToward(Vector2.Zero, PlayerController.friction * (float)delta);
        }

        player.SetVelocity(this, velocity);
        player.MoveAndSlide();

        if (player.Velocity == Vector2.Zero)
        {
            EmitSignal(State.SignalName.Finished, PlayerStates.Idle.ToString(), noData);
        }
    }
}