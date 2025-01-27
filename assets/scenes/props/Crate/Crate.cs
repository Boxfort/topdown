using Godot;
using System;

public partial class Crate : CharacterBody2D
{
    Hurtbox hurtbox;

    public override void _Ready()
    {
        hurtbox = GetNode<Hurtbox>("Hurtbox");
        hurtbox.HitReceived += OnHitReceieved;
    }

    private void OnHitReceieved(AttackData attackData)
    {
        QueueFree();
    }
}
