using Godot;
using System;

public partial class HUD : Control
{
    HealthContainer heartContainer;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        heartContainer = GetNode<HealthContainer>("Health");
        PlayerController player = (PlayerController)GetTree().GetFirstNodeInGroup("player");
        if (player != null) 
        {
            player.HealthChanged += heartContainer.OnHealthChanged;
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
