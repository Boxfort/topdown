using Godot;
using System;

public partial class PlayerSprite : AnimatedSprite2D
{
    [Export]
    Color damageColor = Colors.Red;

    Color defaultColor;

    const float damageVisualTime = 0.2f;
    float damageVisualTimer = 0;
    bool takingDamage = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        defaultColor = Modulate;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (takingDamage)
        {
            damageVisualTimer += (float)delta;
            if (damageVisualTimer >= damageVisualTime)
            {
                Modulate = defaultColor;
                takingDamage = false;
                damageVisualTimer = 0;
            }
        }
    }

    public void OnTakeDamage()
    {
        damageVisualTimer = 0;
        takingDamage = true;
        Modulate = damageColor;
    }
}
