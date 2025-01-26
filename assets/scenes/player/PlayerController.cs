using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class PlayerController : CharacterBody2D
{
    [Signal]
    public delegate void HealthChangedEventHandler(int health);

    CameraShaker cameraShaker;
    PlayerSprite playerSprite;
    Hurtbox hurtbox;
    Area2D playerCollisionArea;
    FootstepAudio footstepAudio;
    CombinedView combinedView;
    TongueScript tongue;

    public const float acceleration = 500.0f;
    public const float maxSpeed = 80.0f;
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
        footstepAudio = GetNode<FootstepAudio>("FootstepAudio");
        combinedView = (CombinedView)GetTree().GetFirstNodeInGroup("combined_view");
        tongue = GetNode<TongueScript>("Tongue");
    }

    private void OnHitReceieved(AttackData attackData)
    {
        if (canBeHit)
        {
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
    float stepDeltaCount = 0;
    const float stepTime = 0.25f;

    public void HandleKnockback(double delta)
    {
        if (knockbackVelocity != Vector2.Zero)
        {
            knockbackVelocity = knockbackVelocity.MoveToward(Vector2.Zero, knockbackVelocityFriction * (float)delta);
        }
    }

    public void HandleWalkingAnimation(State from, double delta, float speedFactor = 1)
    {
        if (from.hasExited) return;

        deltaCount = (deltaCount + (float)(delta * jiggleSpeed * speedFactor)) % 100;
        stepDeltaCount += (float)delta * speedFactor;

        if (Velocity != Vector2.Zero)
        {
            if (stepDeltaCount > stepTime)
            {
                stepDeltaCount = 0;
                footstepAudio.PlayFootstep();
            }
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

        if (lightRecalculateTimer < lightRecalculateTime)
        {
            return;
        }
        else
        {
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
                    lightRadius = gradientTexture.Width / 1.5f;
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
                    // NOTE: the +0.05 is due to the fill of the light going only to 0.9, 0.05f is half of the missing 0.1
                    float samplePoint = Mathf.Clamp((distanceToLight / lightRadius) + 0.05f, 0, 1);

                    // NOTE: using the alpha channel to determine brightness, assume gradient is white->alpha
                    float sample = lightGradient.Sample(samplePoint).A;

                    lightValues.Add(sample * light.Energy);
                }
            }

            float sumOfLight = lightValues.Count > 0 ? Mathf.Min(lightValues.Max(), 1) : 0;
            cornerLightValues.Add(sumOfLight);
        }

        currentLightValue = cornerLightValues.Count > 0 ? cornerLightValues.Average() : 0;

    }
    private Vector2 RoundOffDelta(Vector2 delta)
    {
        var x = delta.X > -0.1 && delta.X < 0.1 ? 0 : delta.X;
        var y = delta.Y > -0.1 && delta.Y < 0.1 ? 0 : delta.Y;

        return new Vector2(x, y);
    }

    public override void _Process(double delta)
    {
        Vector2 mousePosition = combinedView.GetGameWorldMousePosition(GetViewport());
        var dirToMouse = GlobalPosition.DirectionTo(mousePosition);

        if (Input.IsActionJustPressed("alt_fire"))
        {
            tongue.Shoot(dirToMouse);
        }
    }
}
