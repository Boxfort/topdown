using Godot;
using Godot.Collections;
using System;
using System.Threading.Tasks;

public partial class TongueScript : Node2D
{
    Node2D rope;
    Node ropeStart;
    Node ropeEnd;
    Area2D tongueEnd;
    Line2D tongueLine;
    AudioStreamPlayer2DCustom tongueHitAudio;
    AudioStreamPlayer2DCustom tongueStretchAudio;

    Node2D connectedTo;
    const float maxTongueDistance = 64;
    const float minTongueDistance = 16;
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
        tongueHitAudio = GetNode<AudioStreamPlayer2DCustom>("TongueHitAudio");
        tongueStretchAudio = GetNode<AudioStreamPlayer2DCustom>("TongueStretchAudio");

        rope = GetNode<Node2D>("Rope");
        ropeEnd = rope.GetNode("RopeInteractionEnd");
        ropeStart = rope.GetNode("RopeInteractionStart");
    }

    private async Task AttachTongue(Node2D toNode)
    {
        connectedTo = toNode;
        connectedTo.TreeExiting += DetatchTongue;
        rope.Set("rope_length", maxTongueDistance);
        rope.Set("max_endpoint_distance", maxTongueDistance);
        ropeEnd.Set("target_node", toNode);
        ropeEnd.Set("enable", true);
        ropeStart.Set("enable", true);
        rope.Set("pause", false);
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        rope.Show();
    }

    private void DetatchTongue()
    {
        connectedTo.TreeExiting -= DetatchTongue;
        connectedTo.GlobalPosition = connectedTo.GlobalPosition.Round();
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
            bool hitTarget = false;

            if (tongueCollisions.Count > 0)
            {
                foreach (var col in tongueCollisions)
                {
                    if (col is CharacterBody2D body)
                    {
                        AttachTongue(body);
                        isShooting = false;
                        tongueLength = GlobalPosition.DistanceTo(tongueEnd.GlobalPosition);
                        hitTarget = true;
                        tongueHitAudio.Play();
                        break;
                    }
                }

                if (!hitTarget)
                {
                    isReturning = true;
                    isShooting = false;
                }
            }
            else if (tongueEnd.GlobalPosition == tongueTarget || tongueEnd.GlobalPosition.DistanceTo(GlobalPosition) > maxTongueDistance)
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
            tongueLength = Mathf.Min(tongueLength, GlobalPosition.DistanceTo(tongueEnd.GlobalPosition));
            rope.Set("rope_length", tongueLength);
            rope.Set("max_endpoint_distance", tongueLength);

            if (tongueLength <= minTongueDistance)
            {
                DetatchTongue();
                isReturning = true;
            }
        }
        else
        {
            // TODO: without this the rope isn't being hidden, this is a hack
            rope.Hide();
        }

        tongueLine.SetPointPosition(1, tongueLine.ToLocal(tongueEnd.GlobalPosition));
    }

    public void Shoot(Vector2 direction)
    {
        if (isShooting || isReturning) return;

        if (connectedTo != null)
        {
            DetatchTongue();
            isReturning = true;
        }
        else
        {
            tongueTarget = GlobalPosition + (direction * maxTongueDistance) + tongueEndOffset;
            tongueEnd.GlobalPosition = GlobalPosition;
            tongueEnd.Show();
            tongueLine.Show();
            tongueEnd.GlobalPosition = GlobalPosition;
            isShooting = true;
            tongueStretchAudio.Play();
        }
    }
}
