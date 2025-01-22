using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PlayerController : CharacterBody2D
{
    [Signal]
    public delegate void HealthChangedEventHandler(int health);

    CameraShaker cameraShaker;
    PlayerSprite playerSprite;
    Hurtbox hurtbox;
    Area2D playerCollisionArea;

    public const float acceleration = 500.0f;
    public const float maxSpeed = 100.0f;
    public const float friction = 450.0f;
    public const int maxHealth = 6;

    const float lightRecalculateTime = 0.15f;
    float lightRecalculateTimer = 0f;
    float currentLightValue = 0;
    int currentHealth = maxHealth;
    bool canBeHit = true;

    Vector2 knockbackVelocity = Vector2.Zero;
    float knockbackVelocityFriction = 200f;

    readonly List<Vector2> spriteCorners = new() { new(4, -6), new(-4, -6), new(4, 6), new(-4, 6) };
    public AnimatedSprite2D PlayerSprite { get => playerSprite; }
    public float CurrentLightValue { get => currentLightValue; }
    public Vector2 KnockbackVelocity { get => knockbackVelocity; }
    public Area2D PlayerCollisionArea { get => playerCollisionArea; }

    public override void _Ready()
    {
        cameraShaker = (CameraShaker)GetTree().GetFirstNodeInGroup("camera_shaker");
        playerSprite = GetNode<PlayerSprite>("PlayerSprite");
        playerCollisionArea = GetNode<Area2D>("PlayerCollisionArea");
        hurtbox = GetNode<Hurtbox>("Hurtbox");
        hurtbox.HitReceived += OnHitReceieved;
    }

    private void OnHitReceieved(AttackData attackData)
    {
        if (canBeHit) {
            knockbackVelocity = attackData.fromPosition.DirectionTo(GlobalPosition) * attackData.knockbackForce;
            currentHealth = Math.Max(0, currentHealth - attackData.damage);
            EmitSignal(SignalName.HealthChanged, currentHealth);
            playerSprite.OnTakeDamage();

            cameraShaker?.ApplyNoiseShake();
        }
    }

    public void SetVelocity(State from, Vector2 v)
    {
        // We have to check this because of a race condition with multiple states calling Process/PhysicsProcess at the same time.
        // This could be solved by having each state track its own velocity.
        if (from.hasExited) return;

        Velocity = v;
    }

    public const float jiggleSpeed = 20.0f;
    float deltaCount = 0;

    public void HandleKnockback(double delta) 
    {
        if (knockbackVelocity != Vector2.Zero) {
            knockbackVelocity = knockbackVelocity.MoveToward(Vector2.Zero, knockbackVelocityFriction * (float)delta);
        }
    }

    public void HandleWalkingAnimation(double delta)
    {
        deltaCount = (deltaCount + (float)(delta * jiggleSpeed)) % 100;

        if (Velocity != Vector2.Zero)
        {
            float spriteRotation = Mathf.Sin(deltaCount) * 10f;
            playerSprite.RotationDegrees = spriteRotation;
        }
        else
        {
            playerSprite.RotationDegrees = Mathf.Lerp(playerSprite.RotationDegrees, 0, (float)delta * jiggleSpeed);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        CalculateVisiblity(delta);
        HandleKnockback(delta);
    }

    public List<Vector2> GetGlobalSpriteCornerPositions()
    {
        return spriteCorners.Select((x) => x + GlobalPosition).ToList();
    }

    /// <summary>
    /// Caluclates the players current visibility by determining which lights are within range,
    /// then casting a ray from the corners of the players sprite towards each light.
    /// If the ray does not hit anything then that light is lighting the player.
    /// The light is then sampled to get the light value at the players distance from it,
    /// and the 4 corner light values are averaged. The max light value out of all lights is
    /// then used as the final visibility. This is to prevent stacking lights creating more visibility.
    /// </summary>
    public void CalculateVisiblity(double delta)
    {
        lightRecalculateTimer += (float)delta;

        if (lightRecalculateTimer < lightRecalculateTime) {
            return;
        } else {
            lightRecalculateTimer = 0;
        }

        Array<Node> lights = GetTree().GetNodesInGroup("player_light");

        List<float> cornerLightValues = new();

        foreach (Vector2 point in GetGlobalSpriteCornerPositions())
        {
            List<float> lightValues = new(); // List of light values hitting this corner.

            foreach (Node n in lights)
            {
                PointLight2D light = n as PointLight2D;

                float distanceToLight = point.DistanceTo(light.GlobalPosition);
                Texture2D lightTexture = light.Texture;
                float lightRadius = 0;
                Gradient lightGradient = null;

                if (lightTexture is GradientTexture2D gradientTexture)
                {
                    // NOTE: Assume that all the lights will be circular.
                    lightRadius = gradientTexture.Width;
                    lightGradient = gradientTexture.Gradient;
                }
                else
                {
                    GD.Print("NOT CONSIDERING LIGHT '" + light.GetPath() + "' AS IT DOES NOT HAVE A GRADIENT2D TEXTURE");
                    continue;
                }

                if (distanceToLight > lightRadius || !light.Enabled || !light.IsVisibleInTree()) continue;

                var spaceState = GetViewport().GetWorld2D().DirectSpaceState;
                var query = PhysicsRayQueryParameters2D.Create(point, light.GlobalPosition, 0b0000_0010);
                var result = spaceState.IntersectRay(query);

                if (result.Count == 0)
                {
                    float samplePoint = Mathf.Clamp(1 - (distanceToLight / lightRadius), 0, 1);

                    // NOTE: using the red channel to determine brightness, assume gradient is white->black
                    float sample = 1 - lightGradient.Sample(samplePoint).R;

                    lightValues.Add(sample * light.Energy);
                }
            }

            float sumOfLight = lightValues.Count > 0 ? Mathf.Min(lightValues.Sum(), 1) : 0;
            cornerLightValues.Add(sumOfLight);
        }

        currentLightValue = cornerLightValues.Count > 0 ? cornerLightValues.Average() : 0;
    }
}
