using Godot;
using System;

public partial class UIResizeListener : Node
{
    [Export]
    Vector2 designedResolution = new Vector2(1280, 720);

    public Vector2 currentFactor = new(1,1);

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

        currentFactor = new (designedResolution.X / windowSize.X, designedResolution.Y / windowSize.Y);

        viewport.Size = (Vector2I)((Vector2)windowSize * currentFactor.Y);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
