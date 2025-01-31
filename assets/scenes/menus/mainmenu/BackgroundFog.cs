using Godot;
using System;

public partial class BackgroundFog : ColorRect
{
    public override void _Process(double delta)
    { 
        var windowSize = GetViewport().GetWindow().Size;
        ((ShaderMaterial)Material).SetShaderParameter("width", windowSize.X);
        ((ShaderMaterial)Material).SetShaderParameter("height", windowSize.Y);
    }
}
