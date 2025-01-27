using Godot;
using Godot.Collections;
using System;

public partial class GuardDeadState : GuardState
{
    public override void Enter(string previousState, Dictionary data)
    {
        guard.GuardSprite.Play("dead");
        guard.GuardSprite.RotationDegrees = 90;
        guard.CanBeHit = false;
        guard.CollisionLayer = 0b0000_1000;
        guard.CollisionMask = 0b0000_0001;

        guard.WeaponContainer.Hide();
        guard.GuardSprite.GetNode<Node2D>("Torch").Hide();
        guard.SetVelocity(Vector2.Zero);
        guard.QuestionMarkSprite.Hide();
        guard.ExclaimationMarkSprite.Hide();

        guard.Shadow.Position = Vector2.Zero;

        player.StoppedBeingDetectedBy(guard);
    }

    public override void Exit()
    {
        // no-op
    }

    public override void PhysicsProcess(double delta)
    {
        if (guard.KnockbackVelocity != Vector2.Zero)
        {
            guard.Velocity = guard.KnockbackVelocity;
            guard.MoveAndSlide();
        }
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
