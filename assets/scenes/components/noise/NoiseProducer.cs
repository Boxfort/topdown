using Godot;
using System;
using System.Threading.Tasks;

public partial class NoiseProducer : Area2D
{
    [Signal]
    public delegate void NoiseMadeEventHandler();

    CircleShape2D collisionShape;
    const float noiseToRadiusFactor = 2;

    public override void _Ready()
    {
        collisionShape = (CircleShape2D)GetNode<CollisionShape2D>("CollisionShape2D").Shape;
    }

    public async Task TriggerNoise(float amount, Node fromNode)
    {
        collisionShape.Radius = amount * noiseToRadiusFactor;

        // Wait two frames to update the collision shape
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        var areas = GetOverlappingAreas();

        foreach (Area2D area in areas)
        {
            if (area is NoiseListener listener)
            { 
                GD.Print("LISTENER FOUND");

                if(listener.Owner != fromNode)  {
                    listener.HearNoise(GlobalPosition);
                    GD.Print("WAS INVALID");
                }

            }
        }

        EmitSignal(SignalName.NoiseMade);
    }
}
