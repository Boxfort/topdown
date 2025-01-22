using Godot;
using Godot.Collections;
using System;
using System.Linq;

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
        // no-op
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

    Vector2 velocity = Vector2.Zero;

    private void HandleMovement(double delta)
    {
        Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down").Normalized();

        if (direction != Vector2.Zero)
        {
            var velocityThreshold = Vector2.One * PlayerController.maxSpeed * direction.Abs();

            Vector2 newVelocity = velocity + ((float)delta * direction * PlayerController.acceleration);
            newVelocity = newVelocity.Clamp(-velocityThreshold, velocityThreshold);

            velocity = newVelocity;
        }
        else
        {
            velocity = velocity.MoveToward(Vector2.Zero, PlayerController.friction * (float)delta);
        }

        player.SetVelocity(this, velocity + player.KnockbackVelocity);


        // Handling door movement so its not glitchy
        Array<Area2D> areaCollisions = player.PlayerCollisionArea.GetOverlappingAreas();
        foreach (Area2D area in areaCollisions)
        {
            if (area is DoorCollisionArea door)
            {
                door.ParentRigidBody.ApplyCentralForce(player.Velocity.Rotated(door.Rotation) * 25);
            }
        }
        
        var collided = player.MoveAndSlide();

        /*
        if (collided)
        {
            for (int i = 0; i < player.GetSlideCollisionCount(); i++)
            {
                // TODO: Make this not suck
                //       maybe have a 'pushing' state where if we move in the same direction as the object we move it by some factor of its weight vs our speed?'
                //       we can also squish the player sprite in the direction of the push to sell the illusion
                var col = player.GetSlideCollision(i);
                var bounceVelocity = player.Velocity.Bounce(col.GetNormal()) / 2;
                if (col.GetCollider() is RigidBody2D rigidbody)
                {
                    //rigidbody.ApplyCentralForce(col.GetNormal() * - 500);
                }
                player.SetVelocity(bounceVelocity);
                player.MoveAndSlide();
            }
        }
        */

        if (velocity == Vector2.Zero)
        {
            EmitSignal(State.SignalName.Finished, PlayerStates.Idle.ToString(), NO_DATA);
        }
    }
}
