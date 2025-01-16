using Godot;
using System;
using System.Collections.Generic;

public partial class Hitbox : Area2D
{
    [Signal]
    public delegate void HitboxEnteredEventHandler(Hurtbox hurtbox);

    CollisionShape2D collision;
    List<Rid> hasCollidedWith = new();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        collision = GetNode<CollisionShape2D>("CollisionShape2D");
        AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Node2D body)
    {
        if (body is Hurtbox hurtbox && !hasCollidedWith.Contains(hurtbox.GetRid()))
        {
            hasCollidedWith.Add(hurtbox.GetRid());
            EmitSignal(SignalName.HitboxEntered, hurtbox);
        }
    }

    public void SetCollisionDisabled(bool isDisabled)
    {
        if (collision.Disabled != isDisabled)
        {
            hasCollidedWith.Clear();
        }
        collision.Disabled = isDisabled;
    }
}
