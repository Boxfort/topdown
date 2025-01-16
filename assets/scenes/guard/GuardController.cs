using Godot;
using System;

public partial class GuardController : CharacterBody2D
{
    AnimatedSprite2D guardSprite;
    AnimatedSprite2D questionMarkSprite;
    AnimatedSprite2D exclaimationMarkSprite;
    Node2D weaponContainer;
    NavigationAgent2D navAgent;

    public NavigationAgent2D NavAgent { get => navAgent; }
    public AnimatedSprite2D GuardSprite { get => guardSprite; }
    public AnimatedSprite2D QuestionMarkSprite { get => questionMarkSprite; }
    public AnimatedSprite2D ExclaimationMarkSprite { get => exclaimationMarkSprite; }
    public Node2D WeaponContainer { get => weaponContainer; }

    public const float Speed = 50.0f;
    public const float DetectionRadius = 256.0f;
    public const float AttackRange = 24.0f;

    public override void _Ready()
    {
        guardSprite = GetNode<AnimatedSprite2D>("GuardSprite");
        questionMarkSprite = GetNode<AnimatedSprite2D>("Detection/QuestionMark");
        exclaimationMarkSprite = GetNode<AnimatedSprite2D>("Detection/ExclaimationMark");
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

    public bool CanSeeNode(Node2D node)
    {
        if (GlobalPosition.DistanceTo(node.GlobalPosition) <= GuardController.DetectionRadius)
        {
            var spaceState = GetViewport().GetWorld2D().DirectSpaceState;
            var query = PhysicsRayQueryParameters2D.Create(GlobalPosition, node.GlobalPosition, 0b0000_0110);
            var result = spaceState.IntersectRay(query);

            if (result.Count > 0)
            {
                Node2D collidedNode = (Node2D)result["collider"];
                return collidedNode == node;
            }
        }

        return false;
    }

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
