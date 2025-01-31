using Godot;
using System;

public partial class FollowTarget : PointLight2D
{
    [Export(PropertyHint.NodeType, "Node2D")]
    public Node2D target;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (target != null) GlobalPosition = target.GlobalPosition;
    }
}
