using Godot;
using System;
using System.Runtime;

public partial class CameraController : Camera2D
{
    [Export(PropertyHint.NodeType, "Node2D")]
    Node2D cameraTarget;

    [Export]
    CombinedView combinedView;

    float followSpeed = 10f;
    float maxLookDistance = 100f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GlobalPosition = cameraTarget.GlobalPosition;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (cameraTarget != null)
        {
            Vector2 target = cameraTarget.GlobalPosition;

            if (Input.IsActionPressed("look"))
            {
                Vector2 mousePosition = combinedView.GetGameWorldMousePosition(GetViewport());
                Vector2 directionToMouse = target.DirectionTo(mousePosition);

                // Stop the target from moving when the camera moves
                var currentOffset = cameraTarget.GlobalPosition - GlobalPosition;

                float clampedDistance = Mathf.Clamp(target.DistanceTo(mousePosition), -maxLookDistance, maxLookDistance);

                var factor = Mathf.Max(2.5f - (0.5f * combinedView.DesiredZoom), 1f);

                target = cameraTarget.GlobalPosition + (directionToMouse * (target.DistanceTo(mousePosition)/factor)) + currentOffset;
            }

            GlobalPosition = GlobalPosition.Lerp(target, followSpeed * (float)delta);
        }
    }
}
