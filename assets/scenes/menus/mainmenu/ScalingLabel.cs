using Godot;
using System;
using System.Diagnostics;

public partial class ScalingLabel : Label
{
    [Export]
    UIResizeListener resizer;

    public override void _Process(double delta)
    {
        Scale = resizer.currentFactor;
    }
}
