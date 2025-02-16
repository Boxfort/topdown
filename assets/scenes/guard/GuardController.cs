using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

public partial class GuardController : CharacterBody2D
{
    [Export(PropertyHint.NodeType, "NpcPath")]
    NpcPath patrolPath;

    [Export]
    bool canSee = true;

    [Export]
    bool canHear = true;

    Sprite2D playerLastLocationMarker;

    // TODO: this obviously should be a PlayerSprite but im not sure how/if i want to commonise the damage visuals
    PlayerSprite guardSprite;
    AnimatedSprite2D questionMarkSprite;
    AnimatedSprite2D exclaimationMarkSprite;
    Node2D weaponContainer;
    NavigationAgent2D navAgent;
    Hurtbox hurtbox;
    StateMachine stateMachine;
    AnimatedSprite2D knockedOutSprite;
    Sprite2D shadow;
    AudioStreamPlayer2D alertAudio;
    NoiseListener noiseListener;
    Area2D damageableFinder;
    Area2D pathChecker;

    public NavigationAgent2D NavAgent { get => navAgent; }
    public PlayerSprite GuardSprite { get => guardSprite; }
    public AnimatedSprite2D QuestionMarkSprite { get => questionMarkSprite; }
    public AnimatedSprite2D ExclaimationMarkSprite { get => exclaimationMarkSprite; }
    public Node2D WeaponContainer { get => weaponContainer; }
    public Vector2 KnockbackVelocity { get => knockbackVelocity; }
    public bool CanBeHit { get => canBeHit; set => canBeHit = value; }
    public float LastLookAngle { get => lastLookAngle; }
    public NpcPath PatrolPath { get => patrolPath; }
    public AnimatedSprite2D KnockedOutSprite { get => knockedOutSprite; }
    public Sprite2D Shadow { get => shadow; }
    public AudioStreamPlayer2D AlertAudio { get => alertAudio; }
    public Sprite2D PlayerLastLocationMarker { get => playerLastLocationMarker; }
    public Area2D PathChecker { get => pathChecker; }

    public const float Speed = 90.0f;
    public const float DetectionRadius = 256.0f;
    public const float AutomaticDetectionRadius = 32.0f;
    public const float AttackRange = 24.0f;
    public const int maxHealth = 3;

    Vector2 knockbackVelocity = Vector2.Zero;
    float knockbackVelocityFriction = 200f;
    bool canBeHit = true;
    int currentHealth = maxHealth;

    float lastLookAngle = 3f;

    public override void _Ready()
    {
        alertAudio = GetNode<AudioStreamPlayer2D>("AlertAudio");
        guardSprite = GetNode<PlayerSprite>("GuardSprite");
        questionMarkSprite = GetNode<AnimatedSprite2D>("Detection/QuestionMark");
        exclaimationMarkSprite = GetNode<AnimatedSprite2D>("Detection/ExclaimationMark");
        navAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        weaponContainer = GetNode<Node2D>("WeaponContainer");
        stateMachine = GetNode<StateMachine>("StateMachine");
        knockedOutSprite = GetNode<AnimatedSprite2D>("KnockedOutSprite");
        hurtbox = GetNode<Hurtbox>("Hurtbox");
        hurtbox.HitReceived += OnHitReceieved;
        shadow = GetNode<Sprite2D>("Shadow");
        noiseListener = GetNode<NoiseListener>("NoiseListener");
        noiseListener.OnNoiseHeard += OnNoiseHeard;
        damageableFinder = GetNode<Area2D>("DamageableFinder");
        playerLastLocationMarker = GetNode<Sprite2D>("PlayerLastLocationMarker");
        pathChecker = GetNode<Area2D>("PathChecker");

        navAgent.VelocityComputed += OnVelocityComputed;
    }


    bool wasStuck = false;

    private void OnVelocityComputed(Vector2 safeVelocity)
    {
        if (
            stateMachine.CurrentState.Name != "Dead" &&
             stateMachine.CurrentState.Name != "KnockedOut" &&
             stateMachine.CurrentState.Name != "Attacking" &&
             stateMachine.CurrentState.Name != "Alert"
        )
        {
            var requestedVelocity = navAgent.Velocity.Abs();
            Velocity = safeVelocity;
            MoveAndSlide();

            var isStuck = GetRealVelocity().Abs() < requestedVelocity / 7.5f && requestedVelocity > Vector2.Zero;

            // We need to check both as at the end of a patch we can sometimes hit a single frame of 0 safe velocity
            if (isStuck && wasStuck)
            {
                // We're stuck find something to hit
                var areas = damageableFinder.GetOverlappingAreas();
                List<Hurtbox> hitboxes = new();

                foreach (Area2D area in areas)
                {
                    if (area is Hurtbox hitbox && hitbox.Owner is not PlayerController)
                    {
                        hitboxes.Add(hitbox);
                    }
                }

                Hurtbox toHit = hitboxes.MinBy((x) => x.GlobalPosition.DistanceTo(GlobalPosition));

                if (toHit != null)
                {
                    stateMachine.ForceStateSwitch(
                        GuardState.GuardStates.Attacking.ToString(),
                        new Godot.Collections.Dictionary()
                        {
                            ["direction"] = GlobalPosition.DirectionTo(toHit.GlobalPosition)
                        }
                    );
                }
            }

            wasStuck = isStuck;
        }
    }

    private void OnNoiseHeard(Vector2 fromPosition)
    {
        Task.Run(async () =>
        {
            // Wait three frames before responding to the sound
            // This is to stop the guard insta-turning and spotting the player if they swing at them.
            // TODO: maybe set a timer after hearing a noise and then run that?
            await ToSignal(GetTree().CreateTimer(0.25), Timer.SignalName.Timeout);

            if (!canHear) return;

            if (
                    stateMachine.CurrentState.Name != "Dead" &&
                     stateMachine.CurrentState.Name != "KnockedOut" &&
                     stateMachine.CurrentState.Name != "Chase" &&
                     stateMachine.CurrentState.Name != "Alert" &&
                     stateMachine.CurrentState.Name != "Attacking"
                     )
            {

                stateMachine.ForceStateSwitch(GuardState.GuardStates.Investigating.ToString(),
                        new Godot.Collections.Dictionary()
                        {
                            ["investigation_position"] = fromPosition,
                            ["initial_position"] = GlobalPosition
                        }
                    );
            }
        });
    }

    public override void _PhysicsProcess(double delta)
    {
        HandleKnockback(delta);
    }

    private void OnHitReceieved(AttackData attackData)
    {
        if (canBeHit)
        {
            knockbackVelocity = attackData.fromPosition.DirectionTo(GlobalPosition) * attackData.knockbackForce;
            currentHealth = Math.Max(0, currentHealth - attackData.damage);
            guardSprite.OnTakeDamage();

            if (currentHealth == 0)
            {
                stateMachine.ForceStateSwitch(GuardState.GuardStates.Dead.ToString(), State.NO_DATA);
            }

            if (stateMachine.CurrentState is GuardIdleState idle || stateMachine.CurrentState is GuardPatrolState)
            {
                stateMachine.ForceStateSwitch(GuardState.GuardStates.KnockedOut.ToString(), State.NO_DATA);
            }
        }
    }

    public void HandleKnockback(double delta)
    {
        if (knockbackVelocity != Vector2.Zero)
        {
            knockbackVelocity = knockbackVelocity.MoveToward(Vector2.Zero, knockbackVelocityFriction * (float)delta);
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

    public bool CanSeeNode(Node2D node, bool directional)
    {
        if (!canSee) return false;

        if (GlobalPosition.DistanceTo(node.GlobalPosition) <= GuardController.DetectionRadius)
        {
            var lookDir = Vector2.Right.Rotated(lastLookAngle);
            var targetDir = GlobalPosition.DirectionTo(node.GlobalPosition);

            if (directional && lookDir.Dot(targetDir) < 0f) return false; // we're not facing the correct direction

            var spaceState = GetViewport().GetWorld2D().DirectSpaceState;
            var query = PhysicsRayQueryParameters2D.Create(GlobalPosition, node.GlobalPosition, 0b0000_0010);
            var result = spaceState.IntersectRay(query);

            return result.Count == 0;
        }

        return false;
    }

    public float HandleDetection(double delta, float currentDetection, float detectionRate, float detectionLossRate, PlayerController player)
    {
        bool playerSeen = false;

        if (CanSeeNode(player, true))
        {
            if (GlobalPosition.DistanceTo(player.GlobalPosition) <= AutomaticDetectionRadius)
            {
                currentDetection = 1;
                playerSeen = true;
            }
            else if (player.CurrentLightValue > 0.2f)
            {
                //var distanceFactor = 1 - Mathf.Clamp(Mathf.Sqrt(GlobalPosition.DistanceTo(player.GlobalPosition)) / Mathf.Sqrt(GuardController.DetectionRadius), 0, 1);
                var distanceFactor = 1 - Mathf.Clamp(GlobalPosition.DistanceTo(player.GlobalPosition) / GuardController.DetectionRadius, 0, 1);
                currentDetection += (float)delta * detectionRate * distanceFactor * Mathf.Sqrt(player.CurrentLightValue);
                playerSeen = true;
            }
        }

        if (!playerSeen)
        {
            currentDetection = HandleDecreaseDetection(delta, currentDetection, detectionLossRate);
            player.StoppedBeingDetectedBy(this);
        }
        else
        {
            player.StartedBeingDetectedBy(this);
        }

        return currentDetection;
    }

    private float HandleDecreaseDetection(double delta, float currentDetection, float detectionLossRate)
    {
        if (currentDetection > 0)
        {
            currentDetection = Mathf.Max(0, currentDetection - ((float)delta * detectionLossRate));
        }

        return currentDetection;
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

        if (GetRealVelocity() != Vector2.Zero)
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
