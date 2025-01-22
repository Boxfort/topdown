using Godot;
using System;

public partial class GuardController : CharacterBody2D
{
    [Export]
    Texture2D deadSprite;

    [Export]
    Path2D patrolPath;

    // TODO: this obviously should be a PlayerSprite but im not sure how/if i want to commonise the damage visuals
    PlayerSprite guardSprite;
    AnimatedSprite2D questionMarkSprite;
    AnimatedSprite2D exclaimationMarkSprite;
    Node2D weaponContainer;
    NavigationAgent2D navAgent;
    Hurtbox hurtbox;
    StateMachine stateMachine;

    public NavigationAgent2D NavAgent { get => navAgent; }
    public PlayerSprite GuardSprite { get => guardSprite; }
    public AnimatedSprite2D QuestionMarkSprite { get => questionMarkSprite; }
    public AnimatedSprite2D ExclaimationMarkSprite { get => exclaimationMarkSprite; }
    public Node2D WeaponContainer { get => weaponContainer; }
    public Vector2 KnockbackVelocity { get => knockbackVelocity; }
    public Texture2D DeadSprite { get => deadSprite; }
    public bool CanBeHit { get => canBeHit; set => canBeHit = value; }
    public float LastLookAngle { get => lastLookAngle; }
    public Path2D PatrolPath { get => patrolPath; }

    public const float Speed = 90.0f;
    public const float DetectionRadius = 256.0f;
    public const float AttackRange = 24.0f;
    public const int maxHealth = 3;

    Vector2 knockbackVelocity = Vector2.Zero;
    float knockbackVelocityFriction = 200f;
    bool canBeHit = true;
    int currentHealth = maxHealth;

    float lastLookAngle = 3f;

    public override void _Ready()
    {
        guardSprite = GetNode<PlayerSprite>("GuardSprite");
        questionMarkSprite = GetNode<AnimatedSprite2D>("Detection/QuestionMark");
        exclaimationMarkSprite = GetNode<AnimatedSprite2D>("Detection/ExclaimationMark");
        navAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        weaponContainer = GetNode<Node2D>("WeaponContainer");
        stateMachine = GetNode<StateMachine>("StateMachine");
        hurtbox = GetNode<Hurtbox>("Hurtbox");
        hurtbox.HitReceived += OnHitReceieved;
    }

    public override void _PhysicsProcess(double delta)
    {
        HandleKnockback(delta);
    }

    private void OnHitReceieved(AttackData attackData)
    {
        GD.Print("has been hit");
        if (canBeHit)
        {
            knockbackVelocity = attackData.fromPosition.DirectionTo(GlobalPosition) * attackData.knockbackForce;
            currentHealth = Math.Max(0, currentHealth - attackData.damage);
            guardSprite.OnTakeDamage();

            if (currentHealth == 0)
            {
                stateMachine.ForceStateSwitch(GuardState.GuardStates.Dead.ToString(), State.NO_DATA);
            }
        }
    }

    public void HandleKnockback(double delta)
    {
        if (knockbackVelocity != Vector2.Zero)
        {
            knockbackVelocity = knockbackVelocity.MoveToward(Vector2.Zero, knockbackVelocityFriction * (float)delta);
            GD.Print(KnockbackVelocity);
        }
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
            var lookDir = Vector2.Right.Rotated(lastLookAngle);
            var targetDir = GlobalPosition.DirectionTo(node.GlobalPosition);

            if (lookDir.Dot(targetDir) < 0.3f) return false; // we're not facing the correct direction

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

    public void HandleSpriteDirection(float angle)
    {
        lastLookAngle = angle;
        angle = Mathf.RadToDeg(angle);

        string currentAnimation = guardSprite.Animation;
        string desiredAnimation;

        if (angle > -135 && angle < -45)
        {
            desiredAnimation = "back";
        }
        else if (angle < 135 && angle > 45)
        {

            desiredAnimation = "front";
        }
        else
        {
            desiredAnimation = "side";

            if ((angle > -180 && angle < -135) || (angle < 180 && angle > 135))
            {
                // left
                guardSprite.FlipH = true;
            }
            else
            {
                // right
                guardSprite.FlipH = false;
            }
        }

        if (currentAnimation != desiredAnimation)
        {
            guardSprite.Animation = desiredAnimation;
        }
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
