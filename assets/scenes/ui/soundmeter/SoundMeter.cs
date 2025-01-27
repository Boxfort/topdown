using Godot;
using System;

public partial class SoundMeter : Control
{
    PlayerController player;
    AnimatedSprite2D soundMeterSprite;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        player = (PlayerController)GetTree().GetFirstNodeInGroup("player");
        soundMeterSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    }

    public override void _Process(double delta)
    {
        var totalFrames = soundMeterSprite.SpriteFrames.GetFrameCount("default");
        var playerSoundLevel = player.GetNoiseLevel();
        var currentFrame = Mathf.RoundToInt(ScaleInRange(playerSoundLevel, 0, totalFrames, 0, 100));

        soundMeterSprite.SetFrameAndProgress(currentFrame, 0);
    }

    private float ScaleInRange(float input, float newMin, float newMax, float oldMin, float oldMax)
    {
        return (newMax - newMin) * (input - oldMin) / (oldMax - oldMin) + newMin;
    }
}
