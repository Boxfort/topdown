using Godot;
using Godot.Collections;
using System;
using System.Threading.Tasks;

public partial class TongueScript : Node2D
{
    Node2D rope;
    Node ropeStart;
    Node ropeEnd;

    Node2D connectedTo;
    Area2D tongueEnd;
    Line2D tongueLine;
    float tongueDistance = 64;
    Vector2 tongueTarget;
    bool isShooting = false;
    bool isReturning = false;

    float tongueLength = 0;

    Vector2 tongueEndOffset = new Vector2(-1.5f, -1.5f);

    const float tongueSpeed = 300;

    public float TongueLength { get => tongueLength; }

    public override void _Ready()
    {
        tongueEnd = GetNode<Area2D>("TongueEnd");
        tongueLine = GetNode<Line2D>("TongueLine");

        rope = GetNode<Node2D>("Rope");
        ropeEnd = rope.GetNode("RopeInteractionEnd");
        ropeStart = rope.GetNode("RopeInteractionStart");
    }

    private async Task AttachTongue(Node2D toNode)
    {
        connectedTo = toNode;
        ropeEnd.Set("target_node", toNode);
        ropeEnd.Set("enable", true);
        ropeStart.Set("enable", true);
        rope.Set("pause", false);
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        rope.Show();
    }

    private void DetatchTongue()
    {
        ropeEnd.Set("enable", false);
        ropeStart.Set("enable", false);
        rope.Set("pause", true);
        rope.Hide();
        connectedTo = null;
    }

    public override void _Process(double delta)
    {
        if (isShooting)
        {
            tongueEnd.GlobalPosition = tongueEnd.GlobalPosition.MoveToward(tongueTarget, (float)delta * tongueSpeed);
            Array<Node2D> tongueCollisions = tongueEnd.GetOverlappingBodies();

            if (tongueCollisions.Count > 0)
            {
                foreach (var col in tongueCollisions)
                {
                    if (col is CharacterBody2D body)
                    {
                        AttachTongue(body);
                        isShooting = false;
                    }
                }
            }
            else if (tongueEnd.GlobalPosition == tongueTarget || tongueEnd.GlobalPosition.DistanceTo(GlobalPosition) > tongueDistance)
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

        if (connectedTo != null)
        {
            tongueEnd.GlobalPosition = connectedTo.GlobalPosition;
        }

        tongueLine.SetPointPosition(1, tongueLine.ToLocal(tongueEnd.GlobalPosition));
    }

    public void Shoot(Vector2 direction)
    {
        if (connectedTo != null)
        {
            DetatchTongue();
            isReturning = true;
        }
        else
        {
            tongueTarget = GlobalPosition + (direction * tongueDistance) + tongueEndOffset;
            tongueEnd.GlobalPosition = GlobalPosition;
            tongueEnd.Show();
            tongueLine.Show();
            tongueEnd.GlobalPosition = GlobalPosition;
            isShooting = true;
        }
    }
}
