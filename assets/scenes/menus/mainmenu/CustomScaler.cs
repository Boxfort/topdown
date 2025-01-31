using Godot;
using System;
using System.ComponentModel;
using System.Drawing;

public partial class CustomScaler : Node
{
    Vector2 originalSize;
    Control parent;

    [Export]
    float margin = 10;

    [Export]
    Vector2 offset = Vector2.Zero;

    public override void _Ready()
    {
        parent = GetParent<Control>();
        originalSize = parent.Size;
        GetTree().Root.SizeChanged += OnWindowSizeChanged;
        OnWindowSizeChanged();
    }

    private void OnWindowSizeChanged()
    {
        Control parent = GetParent<Control>();
        Vector2I windowSize = GetTree().Root.GetWindow().Size;

        // TODO: New plan we're going to manually place this fucker.
        // Position is width/2 height/2, size is set every frame
        // Scale the x AND y based on the smaller axis, by what % it is different than the designed resolution
        return;

        if (windowSize.X > windowSize.Y)
        {
            // Landscape, limit the Y axis
            var newSize = parent.Size;
            newSize.Y = windowSize.Y;
            newSize.X = newSize.Y * originalSize.Normalized().X;
            parent.CustomMinimumSize = newSize;
        }
        else
        {
            // Portrait
            var newSize = parent.Size;
            newSize.X = windowSize.X;
            newSize.Y = newSize.X * originalSize.Normalized().Y;
            parent.CustomMinimumSize = newSize;
        }
    }

    public override void _Process(double delta)
    {

    }
}
