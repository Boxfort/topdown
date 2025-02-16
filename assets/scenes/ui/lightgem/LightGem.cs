using Godot;
using System;

public partial class LightGem : Control
{
    PlayerController player;
    AnimatedSprite2D lightGemSprite;
    Label debugLabel;

    public PlayerController Player { get => player; set => player = value; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        lightGemSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        debugLabel = GetNode<Label>("Debug");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (player == null) return;

        debugLabel.Text = player.CurrentLightValue.ToString();

        // TODO: do this on a signal rather than setting it every frame
        if (player.CurrentLightValue > 0.8)
        {
            lightGemSprite.SetFrameAndProgress(0, 0);
        }
        else if (player.CurrentLightValue > 0.65)
        {
            lightGemSprite.SetFrameAndProgress(1, 0);
        }
        else if (player.CurrentLightValue > 0.50)
        {
            lightGemSprite.SetFrameAndProgress(2, 0);
        }
        else if (player.CurrentLightValue > 0.35)
        {
            lightGemSprite.SetFrameAndProgress(3, 0);
        }
        else if (player.CurrentLightValue > 0.2)
        {
            lightGemSprite.SetFrameAndProgress(4, 0);
        }
        else
        {
            lightGemSprite.SetFrameAndProgress(5, 0);
        }
    }
}
