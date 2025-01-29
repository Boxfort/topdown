using Godot;
using System;

public partial class FogOfWarTexture : TextureRect
{
    [Export]
    SubViewport visibilityViewport;

    [Export]
    Camera2D playerCamera;

    public override void _Process(double delta)
    {
        Size = visibilityViewport.Size;
        var center = Size/2;
        GlobalPosition = playerCamera.GlobalPosition - center;
    }
}
