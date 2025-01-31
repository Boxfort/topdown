using Godot;
using System;

public partial class Cursor : Control
{
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.ConfinedHidden;
    }

    public override void _Process(double delta)
    {
        var mousePos = GetViewport().GetMousePosition();
        GlobalPosition = mousePos;
    }
}
