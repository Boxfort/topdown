using Godot;
using System;

public partial class DummyCamera : Camera2D
{
    [Export]
    Camera2D mainCamera;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Zoom = mainCamera.Zoom;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        GlobalTransform = mainCamera.GlobalTransform;
        Offset = mainCamera.Offset;
    }
}
