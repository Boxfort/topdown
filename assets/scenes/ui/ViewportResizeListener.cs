using Godot;
using System;

public partial class ViewportResizeListener : Node
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GetTree().Root.SizeChanged += OnWindowSizeChanged;
        OnWindowSizeChanged();
    }

    private void OnWindowSizeChanged()
    {
        SubViewport viewport = GetParent<SubViewport>();
        Vector2I windowSize = GetTree().Root.GetWindow().Size;

        viewport.Size = windowSize;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
