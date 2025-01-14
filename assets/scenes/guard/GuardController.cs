using Godot;
using System;

public partial class GuardController : CharacterBody2D
{
    AnimatedSprite2D guardSprite;
    Node2D weaponContainer;
    NavigationAgent2D navAgent;

    public NavigationAgent2D NavAgent { get => navAgent; }
    public AnimatedSprite2D GuardSprite { get => guardSprite; }
    public Node2D WeaponContainer { get => weaponContainer; }

    public const float Speed = 50.0f;

    public override void _Ready()
    {
        guardSprite = GetNode<AnimatedSprite2D>("GuardSprite");
        navAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        weaponContainer = GetNode<Node2D>("WeaponContainer");
    }

    public void SetVelocity(State from, Vector2 v)
    {
        // We have to check this because of a race condition with multiple states calling Process/PhysicsProcess at the same time.
        // NOTE: this could be fixed by having each state store thier own velocity and then set it, rather than relying on the players
        //       set velocity to be the storage.
        if (from.hasExited) return;
        Velocity = v;
    }

    public const float jiggleSpeed = 20.0f;
    float deltaCount = 0;

    public void HandleWalkingAnimation(double delta)
    {
        deltaCount = (deltaCount + (float)(delta * jiggleSpeed)) % 100;

        if (Velocity != Vector2.Zero)
        {
            float spriteRotation = Mathf.Sin(deltaCount) * 10f;
            guardSprite.RotationDegrees = spriteRotation;
        }
        else
        {
            guardSprite.RotationDegrees = Mathf.Lerp(guardSprite.RotationDegrees, 0, (float)delta * jiggleSpeed);
        }
    }
}
