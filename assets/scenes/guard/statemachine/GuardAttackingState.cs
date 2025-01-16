using Godot;
using Godot.Collections;
using System;

public partial class GuardAttackingState: GuardState
{
    Vector2 direction;

    const float windUpTime = 0.2f;
    float windUpTimer = 0;
    const float attackTime = 0.2f;
    float attackTimer = 0;

    bool startedAttack = false;

    const float intialAttackVelocity = 50f; 
    const float attackVelocityFriction = 100f; 

    public override void Enter(string previousState, Dictionary data)
    {
        direction = (Vector2)data["direction"];
        windUpTimer = 0;
        attackTimer = 0;
        startedAttack = false;

        velocity = intialAttackVelocity * direction;
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

    Vector2 velocity = Vector2.Zero;

    public override void PhysicsProcess(double delta)
    {
        windUpTimer += (float)delta;

        if (windUpTimer >= windUpTime)
        {
            // Do Attack
            if (attackTimer <= attackTime) {
                if (!startedAttack) {
                    guard.WeaponContainer.GetNode<Sword>("Sword").PlayAttack();
                }

                startedAttack = true;
                attackTimer += (float)delta;

                velocity = velocity.MoveToward(Vector2.Zero, attackVelocityFriction * (float)delta);
                guard.SetVelocity(velocity + guard.KnockbackVelocity);
                guard.MoveAndSlide();
            } else {
                EmitSignal(SignalName.Finished, GuardStates.Chase.ToString(), NO_DATA );
            }
        }
    }
}
