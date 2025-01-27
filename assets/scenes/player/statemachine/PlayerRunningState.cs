using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Threading.Tasks;

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
        player.SetVelocity(this, Vector2.Zero);
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
    bool pushedRigidbody = false;

    private void HandleMovement(double delta)
    {
        Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down").Normalized();
        bool isWalking = Input.IsActionPressed("walk");

        if (direction != Vector2.Zero)
        {
            // TODO: put this in a function have some respect
            var walkSpeed = player.WeaponContainer.IsAttacking() ? PlayerController.maxSpeed/3 : (pushedRigidbody || isWalking ? PlayerController.maxSpeed/2 : PlayerController.maxSpeed);

            var velocityThreshold = Vector2.One * walkSpeed * direction.Abs();

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

        pushedRigidbody = false;
        if (collided)
        {
            for (int i = 0; i < player.GetSlideCollisionCount(); i++)
            {
                KinematicCollision2D col = player.GetSlideCollision(i);
                Node2D collider = (Node2D)col.GetCollider();
                if (collider is CharacterBody2D body && body.IsInGroup("pushable"))
                {
                    if (col.GetNormal().Dot(-direction) > 0.5)
                    {
                        GD.Print(col.GetNormal());
                        GD.Print(player.Velocity);
                        GD.Print(col.GetNormal() * player.Velocity/2);
                        body.Velocity = col.GetNormal() * -player.Velocity.Abs()/2;
                        body.MoveAndSlide();
                        player.GlobalPosition += body.GetPositionDelta();
                        // Squish the player when they're pushing the object
                        pushedRigidbody = true;
                        player.PlayerSprite.Scale = Vector2.One - (direction.Abs() / 8);
                        player.PlayerSprite.Position = Vector2.One * direction * 2;
                    }
                }
            }
        }

        if (!pushedRigidbody)
        {
            player.PlayerSprite.Scale = Vector2.One;
            player.PlayerSprite.Position = Vector2.Zero;
        }

        player.HandleWalkingAnimation(this, delta, pushedRigidbody||isWalking ? 0.5f : 1f, pushedRigidbody||isWalking? 0.5f: 1f);

        if (velocity == Vector2.Zero && direction == Vector2.Zero)
        {
            EmitSignal(State.SignalName.Finished, PlayerStates.Idle.ToString(), NO_DATA);
        }
    }
}
