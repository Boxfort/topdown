using Godot;
using System;

public partial class NoiseListener : Area2D
{
    [Signal]
    public delegate void OnNoiseHeardEventHandler(Vector2 fromPosition);

    public bool canHear = true;

    public void HearNoise(Vector2 fromPosition)
    {
        if (canHear) {
            GD.Print(Owner.Name + " heard noise");
            EmitSignal(SignalName.OnNoiseHeard, fromPosition);
        }
    }
}
