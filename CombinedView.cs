using Godot;
using System;

public partial class CombinedView : TextureRect
{
    [Export]
    Vector2 designedResolution = new Vector2(1280, 720);
    [Export]
    float desiredZoom = 2;

    float currentFactor = 1;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Show();
        GetTree().Root.SizeChanged += OnWindowSizeChanged;
        OnWindowSizeChanged();
    }

    public Vector2 GetGameWorldMousePosition(Viewport fromViewport)
    {
        Vector2 viewportMousePos = fromViewport.CanvasTransform.AffineInverse() * fromViewport.GetMousePosition();
        Vector2 cameraPosition = fromViewport.GetCamera2D().Position;

        return (viewportMousePos + (cameraPosition * (Scale.X - 1))) / Scale.X;
    }

    private void OnWindowSizeChanged()
    {
        Vector2I windowSize = GetTree().Root.GetWindow().Size;

        currentFactor = designedResolution.Y / windowSize.Y;

        SetDesiredZoom(desiredZoom);
        Size = windowSize;
        Position = Size * -((Scale.X - 1) * 0.5f);
    }

    public void SetDesiredZoom(float zoom)
    {
        desiredZoom = zoom;
        Vector2 newScale = Vector2.One * desiredZoom * (1 / currentFactor);
        Scale = new Vector2(Mathf.Max(1, newScale.X), Mathf.Max(1, newScale.Y));
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("zoom_in"))
        {
            if (desiredZoom <= 4)
            {
                desiredZoom += 1;
                OnWindowSizeChanged();
            }
        }
        if (Input.IsActionJustPressed("zoom_out"))
        {
            if (desiredZoom > 1)
            {
                desiredZoom -= 1;
                OnWindowSizeChanged();
            }
        }
    }
}
