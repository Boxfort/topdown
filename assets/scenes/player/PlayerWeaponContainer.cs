using Godot;
using System;

public partial class PlayerWeaponContainer : Node2D
{
    CombinedView combinedView;
    Staff staff;

    public override void _Ready()
    {
        combinedView = (CombinedView)GetTree().GetFirstNodeInGroup("combined_view");
        staff = GetNode<Staff>("Staff");
    }

    public override void _Process(double delta)
    {
        Vector2 mousePosition = combinedView.GetGameWorldMousePosition(GetViewport());
        GlobalRotation = GlobalPosition.AngleToPoint(mousePosition) - Mathf.DegToRad(180);

        if (GlobalRotationDegrees > 90 || GlobalRotationDegrees < -90)
        {
            staff.SetFlipV(true);
        }
        else
        {
            staff.SetFlipV(false);
        }

        if (Input.IsActionJustPressed("fire"))
        {
            staff.PlayAttack();
        }
    }
}
