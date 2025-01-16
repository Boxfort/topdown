using Godot;
using System;

public partial class Hurtbox : Area2D
{
    [Signal]
    public delegate void HitReceivedEventHandler(AttackData attackData);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void OnHit(AttackData attackData)
    {
        EmitSignal(SignalName.HitReceived, attackData);
    }
}
