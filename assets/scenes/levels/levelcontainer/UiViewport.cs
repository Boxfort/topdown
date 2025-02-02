using Godot;
using System;

public partial class UiViewport : SubViewport
{
    LightGem lightGem;
    SoundMeter soundMeter;
    HealthContainer heartContainer;

    public override void _Ready()
    {
        lightGem = GetNode<LightGem>("UI/HUD/LightGem");
        soundMeter = GetNode<SoundMeter>("UI/HUD/SoundMeter");
        heartContainer = GetNode<HealthContainer>("UI/HUD/Health");
    }

    public void SetupUIViewport(PlayerController player)
    {
        lightGem.Player = player;
        soundMeter.Player = player;
        player.HealthChanged += heartContainer.OnHealthChanged; 
    }
}
