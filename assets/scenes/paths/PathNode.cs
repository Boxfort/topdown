using Godot;
using Godot.Collections;
using System;

public partial class PathNode : Marker2D
{
    [Export]
    bool shouldFaceDirection = false;

    [Export]
    Vector2 direction;

    [Export]
    bool shouldWait = false;

    [Export]
    float waitTime = 0;

    public bool ShouldFaceDirection { get => shouldFaceDirection; }
    public Vector2 Direction { get => direction; }
    public float WaitTime { get => waitTime; }
    public bool ShouldWait { get => shouldWait; }

    public override void _Ready()
    {
        // TODO: The npc doesn't look in the correct direction if this is not set, this might be down to the greater than comparisons in the sprite check?
        direction += new Vector2(0.001f, 0.001f); 
    }
}
