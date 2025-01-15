using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PlayerController : CharacterBody2D
{
    public const float speed = 100.0f;
    public const float friction = 450.0f;

    const float lightRecalculateTime = 0.15f;
    float lightRecalculateTimer = 0f;

    private float currentLightValue = 0;

    readonly List<Vector2> spriteCorners = new() { new(6, -8), new(-6, -8), new(6, 8), new(-6, 8) };
    AnimatedSprite2D playerSprite;
    public AnimatedSprite2D PlayerSprite { get => playerSprite; }
    public float CurrentLightValue { get => currentLightValue; }

    public override void _Ready()
    {
        playerSprite = GetNode<AnimatedSprite2D>("PlayerSprite");
    }

    public override void _Process(double delta)
    {

    }

    public void SetVelocity(State from, Vector2 v)
    {
        // We have to check this because of a race condition with multiple states calling Process/PhysicsProcess at the same time.
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
    }

    public List<Vector2> GetGlobalSpriteCornerPositions()
    {
        return spriteCorners.Select((x) => x + GlobalPosition).ToList();
    }

    /// <summary>
    /// TODO: Description of how this works
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
