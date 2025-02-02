using Godot;
using System;

public partial class ScalingItem : TextureRect
{
    [Export]
    Vector2 originalSize = new(640, 360);
    float xRatio = 1;
    float yRatio = 1;

    [Export]
    Vector2 offset = Vector2.Zero;

    public override void _Ready()
    {
        yRatio = originalSize.X / originalSize.Y;
        xRatio = originalSize.Y / originalSize.X;
        GetTree().Root.SizeChanged += OnWindowSizeChanged;
        OnWindowSizeChanged();
    }

    private void OnWindowSizeChanged()
    {
        Vector2I windowSize = GetTree().Root.GetWindow().Size;

        // TODO: New plan we're going to manually place this fucker.
        // Position is width/2 height/2, size is set every frame
        // Scale the x AND y based on the smaller axis, by what % it is different than the designed resolution

        if (windowSize.X / windowSize.Y > yRatio)
        {
            // Landscape, limit the Y axis
            var newSize = Size;
            newSize.Y = windowSize.Y;
            newSize.X = newSize.Y * yRatio;
            Size = newSize.Round();
            Position = Vector2.Zero;
            Position = ((windowSize/2) - (Size/2)).Round();
        }
        else
        {
            // Portrait
            var newSize = Size;
            newSize.X = windowSize.X;
            newSize.Y = newSize.X * xRatio;
            Size = newSize.Round();
            Position = Vector2.Zero;
            Position = ((windowSize/2)- (Size/2)).Round();
        }
    }

    public override void _Process(double delta)
    {

    }
}
