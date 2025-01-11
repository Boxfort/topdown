using Godot;
using System;

public partial class CameraShaker : Node2D
{

    // How quickly to move through the noise
    const float NOISE_SHAKE_SPEED = 60.0f;

    // Noise returns values in the range (-1, 1)
    // So this is how much to multiply the returned value by
    const float NOISE_SHAKE_STRENGTH = 5.0f;

    // Multiplier for lerping the shake strength to zero
    const float SHAKE_DECAY_RATE = 10.0f;

    Camera2D camera;
    RandomNumberGenerator rand = new RandomNumberGenerator();
    FastNoiseLite noise = new FastNoiseLite();

    float noiseI = 0;
    float shakeStrength = 0;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        camera = GetParent<Camera2D>();
        rand.Randomize();
        noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
        noise.Seed = (int)rand.Randi();
    }

    public void ApplyNoiseShake() {
        shakeStrength = NOISE_SHAKE_STRENGTH;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        shakeStrength = Mathf.Lerp(shakeStrength, 0, SHAKE_DECAY_RATE * (float)delta);

        camera.Offset = GetNoiseOffset((float)delta);
    }

    public Vector2 GetNoiseOffset(float delta)
    {
        noiseI += delta * NOISE_SHAKE_SPEED;

        return new Vector2(
            noise.GetNoise2D(1, noiseI) * shakeStrength,
            noise.GetNoise2D(100, noiseI) * shakeStrength
        );
    }
}
