using Godot;
using Godot.Collections;
using System;

public partial class TongueScript : Node2D
{
    CharacterBody2D connectedTo;
    Area2D tongueEnd;
    Line2D tongueLine;
    float tongueDistance = 64;
    Vector2 tongueTarget;
    bool isShooting = false;
    bool isReturning = true;

    float tongueLength = 0;

    Vector2 tongueEndOffset = new Vector2(-1.5f, -1.5f);

    const float tongueSpeed = 200;

    public CharacterBody2D ConnectedTo { get => connectedTo; }
    public float TongueLength { get => tongueLength; }

    public override void _Ready()
    {
        tongueEnd = GetNode<Area2D>("TongueEnd");
        tongueLine = GetNode<Line2D>("TongueLine");
    }

    public override void _Process(double delta)
    {
        // This is basically a state machine
        if (isShooting)
        {
            tongueEnd.GlobalPosition = tongueEnd.GlobalPosition.MoveToward(tongueTarget, (float)delta * tongueSpeed);
            Array<Node2D> tongueCollisions = tongueEnd.GetOverlappingBodies();
            foreach (var col in tongueCollisions)
            {
                if (col is CharacterBody2D body)
                {
                    connectedTo = body;
                    isShooting = false;
                    tongueLength = tongueEnd.GlobalPosition.DistanceTo(GlobalPosition);
                }
            }

            if (tongueCollisions.Count == 0 && (tongueEnd.GlobalPosition == tongueTarget || tongueEnd.GlobalPosition.DistanceTo(GlobalPosition) > tongueDistance))
            {
                isReturning = true;
                isShooting = false;
            }

        }
        else if (isReturning)
        {
            tongueEnd.GlobalPosition = tongueEnd.GlobalPosition.MoveToward(GlobalPosition, (float)delta * tongueSpeed);

            if (tongueEnd.GlobalPosition == GlobalPosition)
            {
                isReturning = false;
                tongueEnd.Hide();
                tongueLine.Hide();
            }
        }
        else if (connectedTo != null)
        {
            if (tongueEnd.GlobalPosition.DistanceTo(GlobalPosition) <= 16)
            {
                connectedTo = null;
                tongueEnd.GlobalPosition = GlobalPosition;
                GD.Print("escape");
                tongueEnd.Hide();
                tongueLine.Hide();
            }

            tongueLength = GlobalPosition.DistanceTo(tongueEnd.GlobalPosition);
            tongueEnd.GlobalPosition = connectedTo.GlobalPosition;
        }
        else
        {
            tongueEnd.GlobalPosition = GlobalPosition;
        }

        tongueLine.SetPointPosition(1, tongueLine.ToLocal(tongueEnd.GlobalPosition));
    }

    public void Shoot(Vector2 direction)
    {
        connectedTo = null;
        tongueEnd.GlobalPosition = GlobalPosition;
        tongueTarget = GlobalPosition + (direction * tongueDistance) + tongueEndOffset;
        isShooting = true;
        tongueEnd.GlobalPosition = GlobalPosition;
        tongueEnd.Show();
        tongueLine.Show();
    }
}
