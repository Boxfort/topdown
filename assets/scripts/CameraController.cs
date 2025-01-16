using Godot;
using System;
using System.Runtime;

public partial class CameraController : Camera2D
{
    [Export(PropertyHint.NodeType, "Node2D")]
    Node2D cameraTarget;

    float followSpeed = 400f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GlobalPosition = cameraTarget.GlobalPosition;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (cameraTarget != null && GlobalPosition != cameraTarget.GlobalPosition) {
            GlobalPosition = GlobalPosition.MoveToward(cameraTarget.GlobalPosition, followSpeed*(float)delta);
        }
    }
}
