using Godot;
using System;
using System.Collections;
using System.Threading.Tasks;

public partial class Crate : CharacterBody2D
{
    NoiseProducer noiseProducer;
    Hurtbox hurtbox;

    const float breakNoise = 100;

    public override void _Ready()
    {
        hurtbox = GetNode<Hurtbox>("Hurtbox");
        hurtbox.HitReceived += OnHitReceieved;
        noiseProducer = GetNode<NoiseProducer>("NoiseProducer");
    }

    private void OnHitReceieved(AttackData attackData)
    {
        noiseProducer.NoiseMade += QueueFree;
        _ = noiseProducer.TriggerNoise(breakNoise, attackData.fromNode);
    }
}
