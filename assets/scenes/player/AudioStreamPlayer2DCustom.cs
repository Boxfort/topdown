using Godot;
using System;

public partial class AudioStreamPlayer2DCustom : AudioStreamPlayer2D
{
    [Export(PropertyHint.Range, "0, 2")]
    float minPitch = 0.9f;

    [Export(PropertyHint.Range, "0, 2")]
    float maxPitch = 1.1f;

    protected Random rng = new();

    new public void Play(float fromPosition = 0) 
    {
        PitchScale = (float)(rng.NextDouble() * (maxPitch - minPitch) + minPitch);
        base.Play(fromPosition);
    }
}
