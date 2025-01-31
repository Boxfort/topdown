using Godot;
using System;

public partial class WarningSprite : Sprite2D
{
    public override void _Process(double delta)
    {
        GlobalRotation = 0;
    }
}
