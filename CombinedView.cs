using Godot;
using System;

public partial class CombinedView : TextureRect
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Show();
        GetTree().Root.SizeChanged += OnWindowSizeChanged;
        OnWindowSizeChanged();
    }

    private void OnWindowSizeChanged()
    {
        Vector2I windowSize = GetTree().Root.GetWindow().Size;

        Size = windowSize;
        Position = Size * -((Scale.X-1) * 0.5f);
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("zoom_in")){
            Vector2 currScale = Scale;
            if (currScale.X <= 4){
            currScale += Vector2.One;
            Scale = currScale;
            OnWindowSizeChanged();
            }
        }
        if(Input.IsActionJustPressed("zoom_out"))
        {
            Vector2 currScale = Scale;
            currScale -= Vector2.One;
            if (currScale.X >= 1){
                Scale = currScale;
                OnWindowSizeChanged();
            }
        }
    }
}
