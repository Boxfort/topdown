using Godot;
using System;

public partial class FogOfWarTexture : TextureRect
{
    SubViewport visibilityViewport;
    Camera2D mainCamera;

    public SubViewport VisibilityViewport { get => visibilityViewport; set => visibilityViewport = value; }
    public Camera2D MainCamera { get => mainCamera; set => mainCamera = value; }

    public override void _Process(double delta)
    {
        Size = visibilityViewport.Size;
        var center = Size/2;
        GlobalPosition = mainCamera.GlobalPosition - center;
    }
}
