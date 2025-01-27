using Godot;
using System;

public partial class NoiseProducer : Area2D
{
    CircleShape2D collisionShape;
    const float noiseToRadiusFactor = 2;

    public override void _Ready()
    {
        collisionShape = (CircleShape2D)GetNode<CollisionShape2D>("CollisionShape2D").Shape;
    }

    public void TriggerNoise(float amount)
    {
        collisionShape.Radius = amount * noiseToRadiusFactor;

        var areas = GetOverlappingAreas();

        GD.Print("triggering noise");

        foreach (Area2D area in areas)
        {
            if (area is NoiseListener listener)
            {
                GD.Print("hit listener");
                listener.HearNoise(GlobalPosition);
            }
        }
    }
}
